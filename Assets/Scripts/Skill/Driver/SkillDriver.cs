using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameBase.Log;
using UnityEngine;

public class SkillDriver
{
    private readonly Animator animator;
    private readonly Rigidbody2D rb;
    private readonly Func<float> getDeltaTime;

    public SkillConfig skillConfig { get; private set; } = null;

    private int currentFrame;
    private float frameElapsed;
    private float frameInterval => 1f / skillConfig.FrameRate;

    private bool isPlaying = false;
    private bool isPaused = false;

    public bool IsPlaying => isPlaying;
    public bool IsPaused => isPaused;
    public bool IsCompleted { get; private set; } = false;

    public event Action OnSkillFinished;
    private event Func<int> OnGetFaceDir;
    private event Action<SkillHitBoxClip> OnHitBoxTriggered;
    private event Action OnFacePlayer;
    private List<ISkillTrack> tracks;

    public SkillDriver(Animator animator, Rigidbody2D rb, Action<SkillHitBoxClip> OnHitBoxTriggered, Func<float> getDeltaTime, Func<int> getDir, Action facePlayer)
    {
        this.animator = animator;
        this.rb = rb;
        this.getDeltaTime = getDeltaTime;
        this.OnGetFaceDir = getDir;
        this.OnFacePlayer = facePlayer;
        this.OnHitBoxTriggered = OnHitBoxTriggered;
    }

    public void SetSkill(SkillConfig config)
    {
        skillConfig = config;
        skillConfig.owner = animator? animator.gameObject : rb.gameObject;
        skillConfig.OnGetFaceDir -= this.OnGetFaceDir;
        skillConfig.OnGetFaceDir += this.OnGetFaceDir;
        skillConfig.OwnFacePlayer -= this.OnFacePlayer;
        skillConfig.OwnFacePlayer += this.OnFacePlayer;

        tracks = skillConfig.GetTracks();
        foreach(var t in tracks)
        {
            if(t is SkillAnimationTrack)
            {
                t.Init(skillConfig, animator);
            }
            else if (t is SkillAttackTimeWindowTrack)
            {
                t.Init(skillConfig, null);
            }
            else if(t is SkillHitBoxTrack)
            {
                t.Init(skillConfig, OnHitBoxTriggered);
            }
            else if (t is SkillVelocityTrack)
            {
                t.Init(skillConfig, rb);
            }
        }
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
                foreach(var t in tracks)
                {
                    t.Update(currentFrame);
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
