using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillHitBoxClip : SkillClipBase
{
    [NonSerialized, OdinSerialize]
    public List<Box> HitBoxs = new List<Box>();
    [NonSerialized, OdinSerialize]
    public float HitDamageValue = 1f;
    [NonSerialized, OdinSerialize]
    public LayerMask layer = LayerMask.GetMask("Player");

    [NonSerialized]
    public SkillHitBoxTrack parentTrack;

    [NonSerialized]
    public SkillConfig config;

    public override void OnClipUpdate(int frame)
    {
        base.OnClipUpdate(frame);
        var hasHit = parentTrack.skillConfig.SkillAttackTimeWindowData.HasHit(frame);
        if(!hasHit)
        {
            SkillBoxManager.RegisterHitBox(this, config.skillDriver);
        }
        else
        {
            SkillBoxManager.RegisterHitBox(null, null);
        }
    }
}

public class Box
{
    public Vector2 center;
    public Vector2 size;
    public float rotation;

#if UNITY_EDITOR
    [NonSerialized]
    public int boxIndex = 0;
#endif

    public Box()
    {
        center = Vector2.one;
        size = Vector2.one;
        rotation = 0f;
    }
}

[Serializable]
public class  SkillHitBoxTrack : BaseSkillTrack<SkillHitBoxClip>
{
    public override void Init(SkillConfig config, object o)
    {
        base.Init(config, o);

        foreach (var clip in skillClipDict.Values)
        {
            clip.parentTrack = this;
            clip.config = config;
        }
    }
}
