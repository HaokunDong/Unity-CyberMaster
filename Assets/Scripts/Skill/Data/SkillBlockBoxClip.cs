using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;

public class SkillBlockBoxClip : SkillClipBase
{
    [NonSerialized, OdinSerialize]
    public List<Box> BlockBoxs = new List<Box>();

    [NonSerialized]
    public SkillConfig config;

    public override void OnClipUpdate(int frame)
    {
        base.OnClipUpdate(frame);
        SkillBoxManager.RegisterBlockBox(this, config.skillDriver);
    }
}

[Serializable]
public class SkillBlockBoxTrack : BaseSkillTrack<SkillBlockBoxClip>
{
    public override void Init(SkillConfig config, object o)
    {
        base.Init(config, o);

        foreach (var clip in skillClipDict.Values)
        {
            clip.config = config;
        }
    }

    public override void EmptyClipUpdate()
    {
        SkillBoxManager.RegisterBlockBox(null, null);
    }
}