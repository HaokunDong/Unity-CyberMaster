using System;
using System.Collections.Generic;

public class SkillAttackTimeWindowClip : SkillClipBase
{
}

[Serializable]
public class SkillAttackTimeWindowTrack : BaseSkillTrack<SkillAttackTimeWindowClip>
{

    private Dictionary<SkillAttackTimeWindowClip, bool> hitDict;

    public override void Init(SkillConfig config, object o)
    {
        base.Init(config, o);
        hitDict ??= new();
        hitDict.Clear();
        foreach (var clip in skillClipDict.Values)
        {
            hitDict[clip] = false;
        }
    }

    public bool HasHit(int frame)
    {
        var frameClip = TryGetHitBoxClipAtFrameBinary(frame);
        if (frameClip == null) return false;
        if(hitDict.TryGetValue(frameClip, out var res))
        {
            return res;
        }
        return false;
    }

    public void Hit(int frame)
    {
        var frameClip = TryGetHitBoxClipAtFrameBinary(frame);
        if(frameClip != null)
        {
            hitDict[frameClip] = true;
        }
    }
}