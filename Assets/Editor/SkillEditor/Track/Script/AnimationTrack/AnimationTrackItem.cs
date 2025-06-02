using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AnimationTrackItem : TrackItemBase<AnimationTrack>
{
    private SkillAnimationClip skillAnimationClip;
    public SkillAnimationClip SkillAnimationClip { get => skillAnimationClip; }
    private SingleLineTrackItemStyle trackItemStyle;

    public void Init(AnimationTrack animationTrack, SkillTrackStyleBase parentTrackStyle, int startFrameIndex, float frameUnitWidth, SkillAnimationClip clip)
    {
        track = animationTrack;
        this.frameIndex = startFrameIndex;
        this.frameUnitWidth = frameUnitWidth;
        this.skillAnimationClip = clip;

        itemStyle = trackItemStyle = new SingleLineTrackItemStyle();
        trackItemStyle.Init(parentTrackStyle, startFrameIndex, frameUnitWidth);

        normalColor = new Color(0.388f, 0.850f, 0.905f, 0.5f);
        selectColor = new Color(0.388f, 0.850f, 0.905f, 1f);
        OnUnSelect();

        //绑定事件
        trackItemStyle.mainDragArea.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        trackItemStyle.mainDragArea.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
        trackItemStyle.mainDragArea.RegisterCallback<MouseOutEvent>(OnMouseOutEvent);
        trackItemStyle.mainDragArea.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);

        ResetView(frameUnitWidth);
    }

    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);

        this.frameUnitWidth = frameUnitWidth;
        trackItemStyle.SetTitle(skillAnimationClip.AnimationClip.name);

        //位置计算
        trackItemStyle.SetPosition(frameIndex * frameUnitWidth);
        trackItemStyle.SetWidth(skillAnimationClip.DurationFrame * frameUnitWidth);

        //计算动画结束线的位置
        int animationClipFrameCount = (int)(skillAnimationClip.AnimationClip.length * skillAnimationClip.AnimationClip.frameRate);
        if (animationClipFrameCount > skillAnimationClip.DurationFrame)
        {
            trackItemStyle.overLine.style.display = DisplayStyle.None;
        }
        else
        {
            trackItemStyle.overLine.style.display = DisplayStyle.Flex;
            Vector3 overLinePos = trackItemStyle.overLine.transform.position;
            //overLinePos.x = animationClipFrameCount * frameUnitWidth - animationOverLine.style.width.value.value / 2;
            overLinePos.x = animationClipFrameCount * frameUnitWidth - 1; //线条宽度为2，取一半
            trackItemStyle.overLine.transform.position = overLinePos;
        }

    }


    #region  鼠标交互
    private bool mouseDrag = false;
    private float startDragPosX;
    private int startDragFrameIndex;

    private void OnMouseDownEvent(MouseDownEvent evt)
    {
        startDragPosX = evt.mousePosition.x;
        startDragFrameIndex = frameIndex;
        mouseDrag = true;

        Select();
    }

    private void OnMouseUpEvent(MouseUpEvent evt)
    {
        if (mouseDrag) ApplyDrag();
        mouseDrag = false;
    }

    private void OnMouseOutEvent(MouseOutEvent evt)
    {
        if (mouseDrag) ApplyDrag();
        mouseDrag = false;
    }

    private void OnMouseMoveEvent(MouseMoveEvent evt)
    {
        if (mouseDrag)
        {
            float offsetPos = evt.mousePosition.x - startDragPosX;
            int offsetFrame = Mathf.RoundToInt(offsetPos / frameUnitWidth);
            int targetFrameIndex = startDragFrameIndex + offsetFrame;
            bool checkDrag = false;

            if (targetFrameIndex < 0) return; //不考虑拖拽到负数的情况

            if (offsetFrame < 0)
            {
                checkDrag = track.CheckFrameIndexOnDrag(track.AnimationData.skillAnimationClipDict, targetFrameIndex, startDragFrameIndex, true);
            }
            else if (offsetFrame > 0)
            {
                checkDrag = track.CheckFrameIndexOnDrag(track.AnimationData.skillAnimationClipDict, targetFrameIndex + skillAnimationClip.DurationFrame, startDragFrameIndex, false);
            }
            else return;

            if (checkDrag)
            {
                //确定修改的数据
                frameIndex = targetFrameIndex;

                //如果超过右侧边界，拓展边界
                CheckFrameCount();

                //刷新视图
                ResetView(frameUnitWidth);
            }
        }
    }

    /// <summary>
    /// 如果超过右侧边界，拓展边界
    /// </summary>
    public void CheckFrameCount()
    {
        if (frameIndex + skillAnimationClip.DurationFrame > SkillEditorWindows.Instance.SkillConfig.FrameCount)
        {
            //保存配置导致对象无效，重新引用
            SkillEditorWindows.Instance.CurrentFrameCount = frameIndex + skillAnimationClip.DurationFrame;
        }
    }

    private void ApplyDrag()
    {
        if (startDragFrameIndex != frameIndex)
        {
            track.SetFrameIndex(startDragFrameIndex, frameIndex);
            SkillEditorInspector.Instance.SetTrackItemFrameIndex(frameIndex);
        }
    }

    #endregion

    public override void OnConfigChanged()
    {
        skillAnimationClip = track.AnimationData.skillAnimationClipDict[frameIndex];
    }
}
