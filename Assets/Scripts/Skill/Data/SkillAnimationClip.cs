using GameBase.Log;
using System;
using UnityEngine;

public class SkillAnimationClip : SkillClipBase
{
    public AnimationClip AnimationClip;
    public float TransitionTime = 0f;
    public bool ApplyRootMotion;

    private int startFrame;
    private int durationFrames;
    private float oldSpeed;

    [NonSerialized]
    public SkillAnimationTrack parentTrack;

    public override void OnClipFirstFrame(int frame)
    {
        oldSpeed = parentTrack.animator.speed;
        if (AnimationClip != null)
        {
            LogUtils.Error(AnimationClip.name + " " + Time.time);
            parentTrack.animator.applyRootMotion = ApplyRootMotion;
            startFrame = frame;
            durationFrames = Mathf.RoundToInt(AnimationClip.length * parentTrack.skillConfig.FrameRate);
            parentTrack.animator.Play(AnimationClip.name, 0, 0f); // 
        }
    }

    public override void OnClipUpdate(int frame)
    {
        base.OnClipUpdate(frame);
        if (AnimationClip != null && durationFrames > 0)
        {
            int elapsed = frame - startFrame;
            float normalizedTime = Mathf.Clamp01((float)elapsed / durationFrames);
            parentTrack.animator.Play(AnimationClip.name, 0, normalizedTime);
        }
    }

    public override void OnClipLastFrame(int frame)
    {
        base.OnClipLastFrame(frame);
        parentTrack.animator.speed = oldSpeed;
    }
}

[Serializable]
public class SkillAnimationTrack : BaseSkillTrack<SkillAnimationClip>
{
    [NonSerialized]
    public Animator animator;

    public override void Init(SkillConfig config, object o)
    {
        base.Init(config, o);
        animator = (o as Animator) ?? throw new ArgumentException("Animator is required for SkillAnimationTrack initialization.");

        foreach (var clip in skillClipDict.Values)
        {
            clip.parentTrack = this;
        }
    }
}