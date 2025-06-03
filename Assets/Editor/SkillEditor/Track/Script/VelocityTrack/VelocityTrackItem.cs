public class VelocityTrackItem : TrackItemBase<VelocityTrack, SkillVelocityClip>
{
    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);
        itemStyle.SetTitle(clip.velocity.ToString());
    }
}
