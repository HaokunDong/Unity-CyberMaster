using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTimeWindowTrack : EditorSkillTrackBase<SkillAttackTimeWindowClip>
{
    protected override void CreateItem(int frameIndex, SkillAttackTimeWindowClip clip)
    {
        AttackTimeWindowTrackItem trackItem = new AttackTimeWindowTrackItem();
        trackItem.Init(this, trackStyle, frameIndex, frameWidth, clip, new Color(0.388f, 0.850f, 0.850f, 0.5f), new Color(0.388f, 0.850f, 0.850f, 1f));
        trackItemDic.Add(frameIndex, trackItem.itemStyle);
    }
}
