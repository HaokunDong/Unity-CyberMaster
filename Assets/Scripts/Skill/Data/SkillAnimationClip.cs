using GameBase.Log;
using System;
using UnityEngine;

public class SkillAnimationClip : SkillClipBase
{
    public AnimationClip AnimationClip;
    public float TransitionTime = 0f;
    public bool ApplyRootMotion;

    [NonSerialized]
    public SkillAnimationTrack parentTrack;

    public override void OnClipFirstFrame()
    {
        if (AnimationClip != null)
        {
            LogUtils.Error(AnimationClip.name + " " + Time.time);
            parentTrack.animator.applyRootMotion = ApplyRootMotion;
            parentTrack.animator.CrossFade(AnimationClip.name, TransitionTime);
        }
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