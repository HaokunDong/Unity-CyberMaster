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
    public uint SkillOwnerGPId { get; private set; }
    public Type ShillOwnerGPType { get; private set; }

    private int currentFrame;
    public int CurrentFrame => currentFrame;

    private float frameElapsed;
    private float frameInterval => 1f / skillConfig.FrameRate;

    private bool isPlaying = false;
    private bool isPaused = false;

    public bool IsPlaying
    {
        get => isPlaying;
        private set
        {
            if (isPlaying != value)
            {
                isPlaying = value;
                if (isPlaying)
                {
                    SkillBoxManager.PlayingSkillCount++;
                }
                else
                {
                    SkillBoxManager.PlayingSkillCount--;
                }
            }
        }
    }
    public bool IsPaused => isPaused;

    public event Action OnSkillFinished;
    private event Func<int> OnGetFaceDir;
    public event Action<HitResType, uint, uint, float> OnHitBoxTriggered;
    private event Action OnFacePlayer;
    private List<ISkillTrack> tracks;

    public SkillDriver(uint GPId, Type type, Animator animator, Rigidbody2D rb, Action<HitResType, uint, uint, float> OnHitBoxTriggered, Func<float> getDeltaTime, Func<int> getDir, Action facePlayer)
    {
        this.SkillOwnerGPId = GPId;
        this.ShillOwnerGPType = type;
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
        skillConfig.skillDriver = this;
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
        IsPlaying = true;

        int maxFrame = skillConfig.FrameCount;

        while (IsPlaying && currentFrame <= maxFrame)
        {
            if (isPaused)
            {
                await UniTask.WaitForFixedUpdate();
                continue;
            }

            float delta = getDeltaTime?.Invoke() ?? Time.deltaTime;
            frameElapsed += delta;

            while (frameElapsed >= frameInterval)
            {
                frameElapsed -= frameInterval;
                foreach (var t in tracks)
                {
                    t.Update(currentFrame);
                }

                currentFrame++;
                if (currentFrame > maxFrame)
                {
                    if (skillConfig != null && skillConfig.isLoopSkill)
                    {
                        IsPlaying = false;
                        LoopSkill().Forget();
                    }
                    else
                    {
                        Stop();
                        OnSkillFinished?.Invoke();
                    }
                    break;
                }
            }

            await UniTask.WaitForFixedUpdate();
        }
    }

    private async UniTask LoopSkill()
    {
        await UniTask.DelayFrame(3);
        PlayAsync().Forget();
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
        foreach (var t in tracks)
        {
            t.OnSkillEnd();
        }

        IsPlaying = false;
    }

    public void OnHit(HitResType hitRestype, uint attackerGPId, uint beHitterGPId, float damageBaseValue)
    {
        OnHitBoxTriggered?.Invoke(hitRestype, attackerGPId, beHitterGPId, damageBaseValue);
    }
}
