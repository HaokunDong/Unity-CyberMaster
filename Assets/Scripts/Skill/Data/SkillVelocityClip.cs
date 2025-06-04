using System;
using UnityEngine;

public class SkillVelocityClip : SkillClipBase
{
    public bool FacePlayerFirstFrame = true;
    public bool FacePlayerEveryFrame = false;
    public bool UseCurrentFacing = true;
    public Vector2 velocity;

    [NonSerialized]
    public SkillVelocityTrack parentTrack;

    public override void OnClipFirstFrame()
    {
        base.OnClipFirstFrame();
        if(FacePlayerFirstFrame)
        {
            parentTrack.skillConfig.FacePlayer();
        }
    }

    public override void OnClipUpdate(int frame)
    {
        base.OnClipUpdate(frame);
        if(FacePlayerEveryFrame)
        {
            parentTrack.skillConfig.FacePlayer();
        }
        var face = parentTrack.skillConfig.GetOwnFaceDir();
        parentTrack.rb.velocity = new Vector2(UseCurrentFacing ? velocity.x * face : velocity.x, velocity.y);
    }
}

[Serializable]
public class SkillVelocityTrack : BaseSkillTrack<SkillVelocityClip>
{
    [NonSerialized]
    public Rigidbody2D rb;

    public override void Init(SkillConfig config, object o)
    {
        base.Init(config, o);
        rb = (o as Rigidbody2D) ?? throw new ArgumentException("Rigidbody2D is required for SkillVelocityTrack initialization.");

        foreach (var clip in skillClipDict.Values)
        {
            clip.parentTrack = this;
        }
    }
}
