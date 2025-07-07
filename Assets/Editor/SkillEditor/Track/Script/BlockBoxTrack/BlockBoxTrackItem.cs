public class BlockBoxTrackItem : TrackItemBase<BlockBoxTrack, SkillBlockBoxClip>
{
    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);
        itemStyle.SetTitle(clip.BlockBoxs.Count.ToString());
    }
}
