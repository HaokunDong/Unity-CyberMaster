public class HitBoxTrackItem : TrackItemBase<HitBoxTrack, SkillHitBoxClip>
{
    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);
        itemStyle.SetTitle(clip.HitBoxs.Count.ToString());
    }
}
