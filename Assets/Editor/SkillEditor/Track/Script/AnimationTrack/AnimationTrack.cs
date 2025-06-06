using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AnimationTrack : EditorSkillTrackBase<SkillAnimationClip>
{
    private Vector2 mouseLocalPos;

    protected override void CreateItem(int frameIndex, SkillAnimationClip clip)
    {
        AnimationTrackItem trackItem = new AnimationTrackItem();
        trackItem.Init(this, trackStyle, frameIndex, frameWidth, clip, new Color(0.388f, 0.388f, 0.850f, 0.5f), new Color(0.388f, 0.388f, 0.850f, 1f));
        trackItemDic.Add(frameIndex, trackItem.itemStyle);
    }


    #region  拖拽资源
    protected override void OnDragUpdatedEvent(DragUpdatedEvent evt)
    {
        UnityEngine.Object[] objs = DragAndDrop.objectReferences;
        AnimationClip clip = objs[0] as AnimationClip;
        if (clip != null)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
    }

    protected override void OnPointerDownEvent(MouseDownEvent evt)
    {
        if (evt.button == 1)//右键
        {
            if (!IsClipEmpty(evt))
            {
                return;
            }
            mouseLocalPos = evt.localMousePosition;
            if (SkillEditorWindows.Instance.PreviewCharacterObj != null)
            {
                var animator = SkillEditorWindows.Instance.PreviewCharacterObj.GetComponent<Animator>();
                if (animator != null)
                {
                    RuntimeAnimatorController controller = animator.runtimeAnimatorController;
                    if (controller != null)
                    {
                        HashSet<AnimationClip> clips = new HashSet<AnimationClip>(controller.animationClips);
                        if (clips != null && clips.Count > 0)
                        {
                            ShowContextMenu(evt, clips);
                        }
                    }
                }
            }
        }
    }

    private void ShowContextMenu(MouseDownEvent evt, HashSet<AnimationClip> set)
    {
        GenericMenu menu = new GenericMenu();

        foreach(var ac in set)
        {
            menu.AddItem(new GUIContent(ac.name), false, () =>
            {
                AddAnAnimationClip(ac, SkillEditorWindows.Instance.GetFrameIndexByPos(mouseLocalPos.x));
            });
        }
        menu.DropDown(new Rect(evt.mousePosition, Vector2.zero));
    }

    private void AddAnAnimationClip(AnimationClip clip, int selectFrameIndex)
    {
        bool canPlace = true;
        int durationFrame = -1;//-1 代表可以用原本 AnimationClip 的持续时间
        int clipFrameCount = (int)(clip.length * clip.frameRate);
        int nextTrackItem = -1;
        int currentOffset = int.MaxValue;

        foreach (var item in skillClipDict)
        {
            //不允许选中帧在 TrackItem 中间（动画事件的起点到他的终点之间）
            if (selectFrameIndex > item.Key && selectFrameIndex < item.Key + item.Value.DurationFrame)
            {
                //不能放置
                canPlace = false;
                break;
            }

            //找到右侧的最近 TrackItem
            if (item.Key > selectFrameIndex)
            {
                int tempOffset = item.Key - selectFrameIndex;
                if (tempOffset < currentOffset)
                {
                    currentOffset = tempOffset;
                    nextTrackItem = item.Key;
                }
            }
        }

        //实际的放置
        if (canPlace)
        {
            // 右边有其他 TrackItem ，要考虑 Track 不能重叠的问题
            if (nextTrackItem != -1)
            {
                int offset = clipFrameCount - currentOffset;
                durationFrame = offset < 0 ? clipFrameCount : currentOffset; //计算这个空间能不能完整将动画片段放进去
            }
            else
            {
                //右侧啥都没有
                durationFrame = clipFrameCount;
            }

            //构建动画数据
            SkillAnimationClip skillAnimationClip = new SkillAnimationClip()
            {
                AnimationClip = clip,
                DurationFrame = durationFrame,
                TransitionTime = 0f
            };

            //保存新增的动画数据
            skillClipDict.Add(selectFrameIndex, skillAnimationClip);
            SkillEditorWindows.Instance.SaveConfig();

            //绘制一个Item
            CreateItem(selectFrameIndex, skillAnimationClip);
        }
    }

    protected override void OnDragExitedEvent(DragExitedEvent evt)
    {
        UnityEngine.Object[] objs = DragAndDrop.objectReferences;
        AnimationClip clip = objs[0] as AnimationClip;
        if (clip != null)
        {
            //放置动画资源

            //当前选中的帧数位置 检测是否能放置动画
            int selectFrameIndex = SkillEditorWindows.Instance.GetFrameIndexByPos(evt.localMousePosition.x);
            AddAnAnimationClip(clip, selectFrameIndex);
        }
    }

    #endregion

    public override void TickView(int frameIndex)
    {
        GameObject previewGameObject = SkillEditorWindows.Instance.PreviewCharacterObj;
        Animator animator = previewGameObject.GetComponent<Animator>();

        //根据帧找到目前是哪个动画
        Dictionary<int, SkillAnimationClip> skillAnimationClipDict = skillClipDict;

        #region 关于根运动计算
        SortedDictionary<int, SkillAnimationClip> frameDataSortedDic = new SortedDictionary<int, SkillAnimationClip>(skillAnimationClipDict);
        int[] keys = frameDataSortedDic.Keys.ToArray();
        Vector3 rootMotionTotalPos = Vector3.zero;

        //从第0帧开始累加位移坐标
        for (int i = 0; i < keys.Length; i++)
        {
            int key = keys[i]; //当前动画的起始帧数
            SkillAnimationClip animationEvent = frameDataSortedDic[key];

            //只考虑根运动配置的动画
            if (animationEvent.ApplyRootMotion == false) continue;

            //找到后一个动画的帧起始位置
            int nextKeyFrame = i + 1 < keys.Length ? keys[i + 1] : SkillEditorWindows.Instance.SkillConfig.FrameCount;//最后一个动画

            //标记是最后一次采样
            bool isBreak = false;
            //下一帧大于当前选中帧（帧数累加完成，可以停止累加坐标的标志）
            if (nextKeyFrame > frameIndex)
            {
                nextKeyFrame = frameIndex;
                isBreak = true;
            }

            //持续帧数=下一个动画的帧数  -  这个动画的开始时间
            int durationFrameCount = nextKeyFrame - key;
            if (durationFrameCount > 0)
            {
                //动画资源的总帧数
                float clipFrameCount = animationEvent.AnimationClip.length * SkillEditorWindows.Instance.SkillConfig.FrameRate;
                //计算总的播放进度
                float totalProgress = durationFrameCount / clipFrameCount;
                //播放次数
                int playTimes = 0;
                //最终不完整的一次播放的进度
                float lastProgress = 0;
                //只有循环动画才需要采样多次
                if (animationEvent.AnimationClip.isLooping)
                {
                    playTimes = (int)totalProgress;
                    lastProgress = totalProgress - (int)totalProgress;
                }
                else
                {
                    // 不循环的动画，如果播放进度超过1，约束为1
                    if (totalProgress >= 1)
                    {
                        playTimes = 1;
                        lastProgress = 0;
                    }
                    else if (totalProgress < 1)
                    {
                        lastProgress = totalProgress; // 因为总进度小于1，所以本身就是最后一次播放
                        playTimes = 0;
                    }
                }

                //采样计算
                animator.applyRootMotion = true;
                if (playTimes >= 1)
                {
                    //采样一次动画的完整进度
                    animationEvent.AnimationClip.SampleAnimation(previewGameObject, animationEvent.AnimationClip.length);
                    Vector3 samplePos = previewGameObject.transform.position;
                    rootMotionTotalPos += samplePos * playTimes;
                }

                if (lastProgress > 0)
                {
                    //采样一次动画的不完整进度
                    animationEvent.AnimationClip.SampleAnimation(previewGameObject, lastProgress * animationEvent.AnimationClip.length);
                    Vector3 samplePos = previewGameObject.transform.position;
                    rootMotionTotalPos += samplePos;
                }
            }

            if (isBreak) break;
        }
        #endregion

        #region 关于当前帧的姿态
        //找到距离这一帧左边最近的一个动画，也就是当前要播放的动画
        int currentOffset = int.MaxValue;  //最近的索引距离当前选中帧的差距
        int skillAnimationClipIndex = -1;
        foreach (var item in skillAnimationClipDict)
        {
            int tempOffset = frameIndex - item.Key;
            if (tempOffset > 0 && tempOffset < currentOffset)
            {
                currentOffset = tempOffset;
                skillAnimationClipIndex = item.Key;
            }
        }

        if (skillAnimationClipIndex != -1)
        {
            SkillAnimationClip skillAnimationClip = skillAnimationClipDict[skillAnimationClipIndex];
            //动画资源总帧数
            float clipFrameCount = skillAnimationClip.AnimationClip.length * skillAnimationClip.AnimationClip.frameRate;
            //计算当前的播放进度
            float progress = currentOffset / clipFrameCount;
            //循环动画的处理
            if (progress > 1 && skillAnimationClip.AnimationClip.isLooping)
            {
                progress -= (int)progress;//只保留小数点部分
            }

            //（此处会修改角色位置）
            animator.applyRootMotion = skillAnimationClip.ApplyRootMotion;
            skillAnimationClip.AnimationClip.SampleAnimation(previewGameObject, progress * skillAnimationClip.AnimationClip.length);
        }
        #endregion

        //将角色拉回实际位置
        previewGameObject.transform.position = rootMotionTotalPos;
    }
}
