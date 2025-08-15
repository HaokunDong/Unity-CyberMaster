using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeTrackItem : TrackItemBase<AttributeTrack,SkillAttributeClip>
{
    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);
        itemStyle.SetTitle(" Ù–‘");
    }
}
