using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SkillTrackBase
{
    protected float frameWidth;

    public virtual void Init(VisualElement menuParent, VisualElement trackParent, float frameWidth)
    {
        this.frameWidth = frameWidth;
    }

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

    public virtual void OnConfigChanged() { }

    public virtual void TickView(int frameIndex) { }
    public virtual void Destroy() { }

    public bool CheckFrameIndexOnDrag<T>(Dictionary<int, T> dict, int targetIndex, int selfIndex, bool isLeft) where T : SkillClipBase
    {
        foreach (var item in dict)
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
}
