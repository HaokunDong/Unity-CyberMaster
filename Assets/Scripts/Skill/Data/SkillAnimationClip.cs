using Cysharp.Text;
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

    [NonSerialized]
    public SkillAnimationTrack parentTrack;

    public override void OnClipFirstFrame(int frame)
    {
        if (AnimationClip != null)
        {
            LogUtils.Trace(ZString.Concat("技能驱动动画 ", AnimationClip.name, " 时间 ", Time.time), LogChannel.Battle, new Color(0.388f, 0.388f, 0.850f, 1f));
            parentTrack.animator.applyRootMotion = ApplyRootMotion;
            startFrame = frame;
            durationFrames = Mathf.RoundToInt(AnimationClip.length * parentTrack.skillConfig.FrameRate);
            parentTrack.animator.Play(AnimationClip.name, 0, 0f);
        }
    }

    public override void OnClipUpdate(int frame)
    {
        base.OnClipUpdate(frame);
        if (AnimationClip != null && durationFrames > 0)
        {
            int elapsed = frame - startFrame;
            float normalizedTime = Mathf.Clamp01((float)elapsed / durationFrames);
            parentTrack.animator.Play(AnimationClip.name, parentTrack.skillLayerIndex, normalizedTime);
        }
    }
}

[Serializable]
public class SkillAnimationTrack : BaseSkillTrack<SkillAnimationClip>
{
    [NonSerialized]
    public Animator animator;
    [NonSerialized]
    public int skillLayerIndex;
    private float oldSpeed;

    public override void Init(SkillConfig config, object o)
    {
        base.Init(config, o);
        animator = (o as Animator) ?? throw new ArgumentException("Animator is required for SkillAnimationTrack initialization.");
        oldSpeed = animator.speed;
        animator.speed = 0;
        skillLayerIndex = animator.GetLayerIndex("SkillLayer");

        foreach (var clip in skillClipDict.Values)
        {
            clip.parentTrack = this;
        }
    }

    public override void OnSkillEnd()
    {
        base.OnSkillEnd();
        animator.Play("Empty", skillLayerIndex);
        animator.speed = oldSpeed;
    }
}