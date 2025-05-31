using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SkillDriver
{
    private readonly Animator animator;
    private readonly Func<float> getDeltaTime;

    private SkillConfig skillConfig;
    private SkillAnimationTrack animationTrack;
    private SkillHitBoxTrack hitBoxTrack;

    private int currentFrame;
    private float frameElapsed;
    private float frameInterval => 1f / skillConfig.FrameRate;

    private bool isPlaying = false;
    private bool isPaused = false;

    public bool IsPlaying => isPlaying;
    public bool IsPaused => isPaused;
    public bool IsCompleted { get; private set; } = false;

    public event Action<SkillHitBoxClip> OnHitBoxTriggered;
    public event Action OnSkillFinished;

    public SkillDriver(Animator animator, Func<float> getDeltaTime)
    {
        this.animator = animator;
        this.getDeltaTime = getDeltaTime;
    }

    public void SetSkill(SkillConfig config)
    {
        skillConfig = config;
        animationTrack = config.SkillAnimationData;
        hitBoxTrack = config.SkillHitBoxData;
        hitBoxTrack.BuildFrameToClipMap();
    }

    public async UniTask PlayAsync()
    {
        await PlayFromFrame(0);
    }

    public async UniTask PlayFromFrame(int startFrame)
    {
        currentFrame = Mathf.Clamp(startFrame, 0, skillConfig.FrameCount);
        frameElapsed = 0f;
        isPlaying = true;
        IsCompleted = false;

        int maxFrame = skillConfig.FrameCount;

        while (isPlaying && currentFrame <= maxFrame)
        {
            if (isPaused)
            {
                await UniTask.Yield();
                continue;
            }

            float delta = getDeltaTime?.Invoke() ?? Time.deltaTime;
            frameElapsed += delta;

            while (frameElapsed >= frameInterval)
            {
                frameElapsed -= frameInterval;

                // 播放动画
                if (animationTrack.skillAnimationClipDict.TryGetValue(currentFrame, out var animClip))
                {
                    animator.applyRootMotion = animClip.ApplyRootMotion;
                    animator.CrossFade(animClip.AnimationClip.name, animClip.TransitionTime);
                }

                // 打击触发
                var hitClip = hitBoxTrack.TryGetHitBoxClipAtFrameBinary(currentFrame);
                if (hitClip != null)
                {
                    OnHitBoxTriggered?.Invoke(hitClip);
                }

                currentFrame++;
                if (currentFrame > maxFrame)
                {
                    isPlaying = false;
                    IsCompleted = true;
                    OnSkillFinished?.Invoke();
                    break;
                }
            }

            await UniTask.Yield();
        }
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
    }

    public void Stop()
    {
        isPlaying = false;
        IsCompleted = true;
    }
}
