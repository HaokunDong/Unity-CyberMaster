using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityTrack : EditorSkillTrackBase<SkillVelocityClip>
{
    protected override void CreateItem(int frameIndex, SkillVelocityClip clip)
    {
        VelocityTrackItem trackItem = new VelocityTrackItem();
        trackItem.Init(this, trackStyle, frameIndex, frameWidth, clip, new Color(0.388f, 0.850f, 0.388f, 0.5f), new Color(0.388f, 0.850f, 0.388f, 1f));
        trackItemDic.Add(frameIndex, trackItem.itemStyle);
    }
}
