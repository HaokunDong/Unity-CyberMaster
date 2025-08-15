using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAttributeClip : SkillClipBase
{
    public bool IsInvincible = true;

    [NonSerialized]
    public SkillAttributeTrack parentTrack;

    public override void OnClipFirstFrame(int frame)
    {
        base.OnClipFirstFrame(frame);
        if (IsInvincible)
        {
            //parentTrack.skillConfig
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
