using UnityEngine;

public class HitBoxTrack : EditorSkillTrackBase<SkillHitBoxClip>
{
    protected override void CreateItem(int frameIndex, SkillHitBoxClip clip)
    {
        HitBoxTrackItem trackItem = new HitBoxTrackItem();
        trackItem.Init(this, trackStyle, frameIndex, frameWidth, clip, new Color(0.850f, 0.388f, 0.388f, 0.5f), new Color(0.850f, 0.388f, 0.388f, 1f));
        trackItemDic.Add(frameIndex, trackItem.itemStyle);
    }
}