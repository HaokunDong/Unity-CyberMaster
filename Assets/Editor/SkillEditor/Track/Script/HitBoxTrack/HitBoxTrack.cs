using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.UIElements;

public class HitBoxTrack : SkillTrackBase
{
    private SkillSingleLineTrackStyle trackStyle;

    private Dictionary<int, HitBoxTrackItem> trackItemDic = new Dictionary<int, HitBoxTrackItem>();
    public SkillHitBoxTrack HitBoxData { get => SkillEditorWindows.Instance.SkillConfig.SkillHitBoxData; }

    public override void Init(VisualElement menuParent, VisualElement trackParent, float frameWidth)
    {
        base.Init(menuParent, trackParent, frameWidth);
        trackStyle = new SkillSingleLineTrackStyle();
        trackStyle.Init(menuParent, trackParent, "打击");
        trackStyle.contentRoot.RegisterCallback<MouseDownEvent>(OnPointerDownEvent);

        ResetView();
    }

    private void CreateItem(int frameIndex, SkillHitBoxClip clip)
    {
        HitBoxTrackItem trackItem = new HitBoxTrackItem();
        trackItem.Init(this, trackStyle, frameIndex, frameWidth, clip);
        trackItemDic.Add(frameIndex, trackItem);
    }

    private void OnPointerDownEvent(MouseDownEvent evt)
    {
        if(evt.button == 1)//右键
        {
            int selectFrameIndex = SkillEditorWindows.Instance.GetFrameIndexByPos(evt.localMousePosition.x);

            SkillHitBoxClip clip = new SkillHitBoxClip()
            {
                DurationFrame = 1,
                HitBoxs = new List<Box>(),
            };

            //保存新增的动画数据
            HitBoxData.skillClipDict.Add(selectFrameIndex, clip);
            SkillEditorWindows.Instance.SaveConfig();

            //绘制一个Item
            CreateItem(selectFrameIndex, clip);
        }
    }

    public override void ResetView(float frameWidth)
    {
        base.ResetView(frameWidth);
        foreach (var item in trackItemDic)
        {
            trackStyle.DeleteItem(item.Value.itemStyle.root);
        }

        trackItemDic.Clear();
        if (SkillEditorWindows.Instance.SkillConfig == null) return;

        //根据数据绘制 TrackItem
        foreach (var item in HitBoxData.skillClipDict)
        {
            CreateItem(item.Key, item.Value);
        }
    }

    public void SetFrameIndex(int oldIndex, int newIndex)
    {
        if (HitBoxData.skillClipDict.Remove(oldIndex, out SkillHitBoxClip clip))
        {
            HitBoxData.skillClipDict.Add(newIndex, clip);
            trackItemDic.Remove(oldIndex, out HitBoxTrackItem item);
            trackItemDic.Add(newIndex, item);

            SkillEditorWindows.Instance.SaveConfig();
        }
    }

    public override void DeleteTrackItem(int frameIndex)
    {
        //移除数据
        HitBoxData.skillClipDict.Remove(frameIndex);
        if (trackItemDic.Remove(frameIndex, out HitBoxTrackItem item))
        {
            //移除视图
            trackStyle.DeleteItem(item.itemStyle.root);
        }
        SkillEditorWindows.Instance.SaveConfig();
    }

    public override void TickView(int frameIndex)
    {
        base.TickView(frameIndex);

        int currentOffset = int.MaxValue;  //最近的索引距离当前选中帧的差距
        int clipIndex = -1;
        foreach (var item in HitBoxData.skillClipDict)
        {
            int tempOffset = frameIndex - item.Key;
            if (tempOffset > 0 && tempOffset < currentOffset)
            {
                currentOffset = tempOffset;
                clipIndex = item.Key;
            }
        }

        if(clipIndex >= 0)
        {
            SkillHitBoxClip clip = HitBoxData.skillClipDict[clipIndex];
        }
    }
}
