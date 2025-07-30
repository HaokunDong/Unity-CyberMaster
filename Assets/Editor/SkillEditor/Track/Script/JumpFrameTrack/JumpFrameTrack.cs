using UnityEngine;

public class JumpFrameTrack : EditorSkillTrackBase<SkillJumpFrameClip>
{
    protected override int GetNewMinFrameCount => 1;

    protected override void CreateItem(int frameIndex, SkillJumpFrameClip clip)
    {
        JumpFrameTrackItem trackItem = new JumpFrameTrackItem();
        trackItem.Init(this, trackStyle, frameIndex, frameWidth, clip, new Color(1f, 0, 0, 0.5f), new Color(1f, 0, 0, 1f));
        trackItemDic.Add(frameIndex, trackItem.itemStyle);
    }
}
