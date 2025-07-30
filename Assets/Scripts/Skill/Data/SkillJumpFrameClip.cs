using System;

public class SkillJumpFrameClip : SkillClipBase
{
    [NonSerialized]
    public SkillConfig config;

    public int Jump2Frame = -1;

    public override void OnClipLastFrame(int frame)
    {
        base.OnClipLastFrame(frame);
        if(Jump2Frame >= 0)
        {
            config.skillDriver.JumpToFrame(Jump2Frame);
        }
    }
}

[Serializable]
public class SkillJumpFrameTrack : BaseSkillTrack<SkillJumpFrameClip>
{
    public override void Init(SkillConfig config, object o)
    {
        base.Init(config, o);

        foreach (var clip in skillClipDict.Values)
        {
            clip.config = config;
        }
    }
}