public class JumpFrameTrackItem : TrackItemBase<JumpFrameTrack, SkillJumpFrameClip>
{
    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);
        itemStyle.SetTitle(clip.Jump2Frame.ToString());
    }
}
