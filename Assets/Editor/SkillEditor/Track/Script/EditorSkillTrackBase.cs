using System.Collections.Generic;
using UnityEngine.UIElements;

public abstract class EditorSkillTrackBase
{
    protected float frameWidth;
    protected SkillSingleLineTrackStyle trackStyle;
    protected Dictionary<int, SkillTrackItemStyleBase> trackItemDic = new ();

    /// <summary>
    /// 在当前宽高进行刷新（内部变化）
    /// </summary>
    public virtual void ResetView()
    {
        ResetView(frameWidth);
    }

    /// <summary>
    /// 宽高有变化的刷新（滚轮滑动）
    /// </summary>
    /// <param name="frameWidth"></param>
    public virtual void ResetView(float frameWidth)
    {
        this.frameWidth = frameWidth;
    }

    public virtual void DeleteTrackItem(int frameIndex) { }
    public virtual void TickView(int frameIndex) { }
    public virtual void Destroy() 
    {
        trackStyle?.Destroy();
    }

    public virtual bool CheckFrameIndexOnDrag(int targetIndex, int selfIndex, bool isLeft)
    {
        return true;
    }

    public virtual void SetFrameIndex(int oldIndex, int newIndex)
    {

    }

    public virtual SkillClipBase GetClip(int frameIndex)
    {
        return null;
    }
}

public abstract class EditorSkillTrackBase<SCB> : EditorSkillTrackBase where SCB : SkillClipBase
{
    public Dictionary<int, SCB> skillClipDict;

    public virtual void Init(VisualElement menuParent, VisualElement trackParent, float frameWidth, Dictionary<int, SCB> dict, string title)
    {
        this.frameWidth = frameWidth;
        this.skillClipDict = dict;
        this.trackStyle = new SkillSingleLineTrackStyle();
        trackStyle.Init(menuParent, trackParent, title);
        trackStyle.contentRoot.RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
        trackStyle.contentRoot.RegisterCallback<DragExitedEvent>(OnDragExitedEvent);
        trackStyle.contentRoot.RegisterCallback<MouseDownEvent>(OnPointerDownEvent);

        ResetView();
    }

    public override void ResetView(float frameWidth)
    {
        base.ResetView(frameWidth);
        foreach (var item in trackItemDic)
        {
            trackStyle.DeleteItem(item.Value.root);
        }

        trackItemDic.Clear();
        if (SkillEditorWindows.Instance.SkillConfig == null) return;

        //根据数据绘制 TrackItem
        foreach (var item in skillClipDict)
        {
            CreateItem(item.Key, item.Value);
        }
    }

    public override void SetFrameIndex(int oldIndex, int newIndex)
    {
        if (skillClipDict.Remove(oldIndex, out SCB clip))
        {
            skillClipDict.Add(newIndex, clip);
            trackItemDic.Remove(oldIndex, out var style);
            trackItemDic.Add(newIndex, style);

            SkillEditorWindows.Instance.SaveConfig();
        }
    }

    public override void DeleteTrackItem(int frameIndex)
    {
        //移除数据
        skillClipDict.Remove(frameIndex);
        if (trackItemDic.Remove(frameIndex, out var style))
        {
            //移除视图
            trackStyle.DeleteItem(style.root);
        }
        SkillEditorWindows.Instance.SaveConfig();
    }

    protected abstract void CreateItem(int frameIndex, SCB clip);

    protected virtual void OnDragUpdatedEvent(DragUpdatedEvent evt)
    {
    }

    protected virtual void OnDragExitedEvent(DragExitedEvent evt)
    {
    }

    protected virtual void OnPointerDownEvent(MouseDownEvent evt)
    {
        if (evt.button == 1)//右键
        {
            int selectFrameIndex = SkillEditorWindows.Instance.GetFrameIndexByPos(evt.localMousePosition.x);

            SCB clip = System.Activator.CreateInstance<SCB>();
            clip.DurationFrame = 5; //默认持续时间为5帧
            //保存新增的动画数据
            skillClipDict.Add(selectFrameIndex, clip);
            SkillEditorWindows.Instance.SaveConfig();

            //绘制一个Item
            CreateItem(selectFrameIndex, clip);
        }
    }

    public override void TickView(int frameIndex)
    {
        int currentOffset = int.MaxValue;  //最近的索引距离当前选中帧的差距
        int clipIndex = -1;
        foreach (var item in skillClipDict)
        {
            int tempOffset = frameIndex - item.Key;
            if (tempOffset > 0 && tempOffset < currentOffset)
            {
                currentOffset = tempOffset;
                clipIndex = item.Key;
            }
        }

        if (clipIndex >= 0)
        {
            SCB clip = skillClipDict[clipIndex];
        }
    }

    public override bool CheckFrameIndexOnDrag(int targetIndex, int selfIndex, bool isLeft)
    {
        foreach (var item in skillClipDict)
        {
            //拖拽时，规避自身
            if (item.Key == selfIndex) continue;

            //向左移动&&原先在右边&&目标没有重叠
            if (isLeft && selfIndex > item.Key && targetIndex < item.Key + item.Value.DurationFrame)
            {
                return false;
            }
            //向右移动&&原先在左边&&目标没有重叠
            else if (!isLeft && selfIndex < item.Key && targetIndex > item.Key)
            {
                return false;
            }
        }

        return true;
    }

    public override SkillClipBase GetClip(int frameIndex)
    {
        return skillClipDict[frameIndex];
    }
}
