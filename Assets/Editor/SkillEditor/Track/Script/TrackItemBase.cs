using UnityEngine;
using UnityEngine.UIElements;

public abstract class TrackItemBase
{
    public SkillTrackItemStyleBase itemStyle { get; protected set; }
    protected float frameUnitWidth;
    protected int frameIndex;
    public int FrameIndex { get => frameIndex; }
    public abstract void Select();
    public abstract void OnSelect();
    public abstract void OnUnSelect();
    public virtual void OnConfigChanged() { }
    public void ResetView()
    {
        ResetView(frameUnitWidth);
    }
    public virtual void ResetView(float frameUnitWidth)
    {
        this.frameUnitWidth = frameUnitWidth;
    }
}

public abstract class TrackItemBase<T, SCB> : TrackItemBase where T : EditorSkillTrackBase where SCB : SkillClipBase
{
    public SCB Clip { get => clip; }
    protected SCB clip;
    
    protected Color normalColor = new Color(0.388f, 0.850f, 0.905f, 0.5f);
    protected T track;

    protected Color selectColor = new Color(0.388f, 0.850f, 0.905f, 1f);

    protected bool mouseDrag = false;
    protected float startDragPosX;
    protected int startDragFrameIndex;


    public virtual void Init(T Track, SkillTrackStyleBase parentTrackStyle, int startFrameIndex, float frameUnitWidth, SCB clip, Color colorN, Color colorS)
    {
        this.track = Track;
        this.frameIndex = startFrameIndex;
        this.frameUnitWidth = frameUnitWidth;
        this.clip = clip;

        itemStyle = new SingleLineTrackItemStyle();
        itemStyle.Init(parentTrackStyle, startFrameIndex, frameUnitWidth);

        normalColor = colorN;
        selectColor = colorS;
        OnUnSelect();

        //绑定事件
        itemStyle.mainDragArea.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        itemStyle.mainDragArea.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
        itemStyle.mainDragArea.RegisterCallback<MouseOutEvent>(OnMouseOutEvent);
        itemStyle.mainDragArea.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);

        ResetView(frameUnitWidth);
    }

    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);

        //位置计算
        itemStyle.SetPosition(frameIndex * frameUnitWidth);
        itemStyle.SetWidth(clip.DurationFrame * frameUnitWidth);

        //计算动画结束线的位置
        int clipFrameCount = clip.DurationFrame;
        itemStyle.overLine.style.display = DisplayStyle.Flex;
        Vector3 overLinePos = itemStyle.overLine.transform.position;
        //overLinePos.x = animationClipFrameCount * frameUnitWidth - animationOverLine.style.width.value.value / 2;
        overLinePos.x = clipFrameCount * frameUnitWidth - 1; //线条宽度为2，取一半
        itemStyle.overLine.transform.position = overLinePos;
    }

    public override void Select()
    {
        SkillEditorWindows.Instance.ShowTrackItemOnInspector(this, track);
    }

    public override void OnSelect()
    {
        SkillEditorWindows.selectedTrackItem = this;
        itemStyle.SetBGColor(selectColor);
    }

    public override void OnUnSelect()
    {
        SkillEditorWindows.selectedTrackItem = null;
        itemStyle.SetBGColor(normalColor);
    }

    /// <summary>
    /// 如果超过右侧边界，拓展边界
    /// </summary>
    public void CheckFrameCount()
    {
        if (frameIndex + clip.DurationFrame > SkillEditorWindows.Instance.SkillConfig.FrameCount)
        {
            //保存配置导致对象无效，重新引用
            SkillEditorWindows.Instance.CurrentFrameCount = frameIndex + clip.DurationFrame;
        }
    }

    protected void ApplyDrag()
    {
        if (startDragFrameIndex != frameIndex)
        {
            track.SetFrameIndex(startDragFrameIndex, frameIndex);
            SkillEditorInspector.Instance.SetTrackItemFrameIndex(frameIndex);
        }
    }

    protected virtual void OnMouseDownEvent(MouseDownEvent evt)
    {
        startDragPosX = evt.mousePosition.x;
        startDragFrameIndex = frameIndex;
        mouseDrag = true;

        Select();
    }

    protected virtual void OnMouseUpEvent(MouseUpEvent evt)
    {
        if (mouseDrag) ApplyDrag();
        mouseDrag = false;
    }

    protected virtual void OnMouseOutEvent(MouseOutEvent evt)
    {
        if (mouseDrag) ApplyDrag();
        mouseDrag = false;
    }

    protected virtual void OnMouseMoveEvent(MouseMoveEvent evt)
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
                checkDrag = track.CheckFrameIndexOnDrag(targetFrameIndex, startDragFrameIndex, true);
            }
            else if (offsetFrame > 0)
            {
                checkDrag = track.CheckFrameIndexOnDrag(targetFrameIndex + clip.DurationFrame, startDragFrameIndex, false);
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

    public override void OnConfigChanged()
    {
        clip = (SCB)(track.GetClip(frameIndex));
    }
}
