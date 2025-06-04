public class AttackTimeWindowTrackItem : TrackItemBase<AttackTimeWindowTrack, SkillAttackTimeWindowClip>
{
    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);
        itemStyle.SetTitle(clip.DurationFrame.ToString());
    }
}
