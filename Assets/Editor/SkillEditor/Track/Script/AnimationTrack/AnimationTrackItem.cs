using UnityEngine;
using UnityEngine.UIElements;

public class AnimationTrackItem : TrackItemBase<AnimationTrack, SkillAnimationClip>
{
    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);
        itemStyle.SetTitle(clip.AnimationClip.name);

        //计算动画结束线的位置
        int animationClipFrameCount = (int)(clip.AnimationClip.length * clip.AnimationClip.frameRate);
        if (animationClipFrameCount > clip.DurationFrame)
        {
            itemStyle.overLine.style.display = DisplayStyle.None;
        }
        else
        {
            itemStyle.overLine.style.display = DisplayStyle.Flex;
            Vector3 overLinePos = itemStyle.overLine.transform.position;
            //overLinePos.x = animationClipFrameCount * frameUnitWidth - animationOverLine.style.width.value.value / 2;
            overLinePos.x = animationClipFrameCount * frameUnitWidth - 1; //线条宽度为2，取一半
            itemStyle.overLine.transform.position = overLinePos;
        }
    }
}
