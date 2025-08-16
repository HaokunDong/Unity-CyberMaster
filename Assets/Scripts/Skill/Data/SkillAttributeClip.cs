using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAttributeClip : SkillClipBase
{
    public List<AddSkillAttribute> AddSkillAttributes;

    [NonSerialized]
    public SkillAttributeTrack parentTrack;

    public override void OnClipUpdate(int frame)
    {
        base.OnClipUpdate(frame);
        if(AddSkillAttributes != null)
        {
            var len = AddSkillAttributes.Count;
            for(int i = 0; i < len; i++)
            {
                var asa = AddSkillAttributes[i];
                parentTrack.skillConfig.skillDriver.AddAttribute(asa);
            }
        }
    }
}

[Serializable]
public class SkillAttributeTrack : BaseSkillTrack<SkillAttributeClip>
{
    public override void Init(SkillConfig config, object o)
    {
        base.Init(config, o);

        foreach (var clip in skillClipDict.Values)
        {
            clip.parentTrack = this;
        }
    }
}
