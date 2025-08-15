using UnityEngine;

public class AttributeTrack : EditorSkillTrackBase<SkillAttributeClip>
{
    protected override void CreateItem(int frameIndex, SkillAttributeClip clip)
    {
        AttributeTrackItem trackItem = new AttributeTrackItem();
        trackItem.Init(this, trackStyle, frameIndex, frameWidth, clip, new Color(0.388f, 0.850f, 0.388f, 0.5f), new Color(0.388f, 0.850f, 0.388f, 1f));
        trackItemDic.Add(frameIndex, trackItem.itemStyle);
    }
}
