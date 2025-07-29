using System;
using System.Collections.Generic;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Managers;
using Sirenix.Utilities;
using UnityEngine;

public interface ISkillDriverUnit
{
    public SkillDriver skillDriverImp { get; }
}

public class SkillDriver
{
    private readonly Animator animator;
    private readonly Rigidbody2D rb;
    private readonly Func<float> getDeltaTime;
    private GamePlayEntity owner;

    public SkillConfig skillConfig { get; private set; } = null;
    public uint SkillOwnerGPId { get; private set; }
    public Type ShillOwnerGPType { get; private set; }

    private int currentFrame;
    public int CurrentFrame => currentFrame;

    private float frameElapsed;
    private float frameInterval => 1f / skillConfig.FrameRate;

    private bool isPlaying = false;
    private bool isPaused = false;
    private string bufferedSkillName = null;

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

    public SkillDriver(GamePlayEntity entity, Type type, Animator animator, Rigidbody2D rb, Action<HitResType, uint, uint, float> OnHitBoxTriggered, Func<float> getDeltaTime, Func<int> getDir, Action facePlayer)
    {
        this.owner = entity;
        this.SkillOwnerGPId = entity.GamePlayId;
        this.ShillOwnerGPType = type;
        this.animator = animator;
        this.rb = rb;
        this.getDeltaTime = getDeltaTime;
        this.OnGetFaceDir = getDir;
        this.OnFacePlayer = facePlayer;
        this.OnHitBoxTriggered = OnHitBoxTriggered;
    }

    public void BufferNextSkill(string skillName)
    {
        // 如果当前技能不允许接这个技能，也可以在这里判断
        bufferedSkillName = skillName;
    }

    public void SetSkill(SkillConfig config)
    {
        skillConfig = config;
        skillConfig.owner = owner;
        skillConfig.skillDriver = this;
        skillConfig.OnGetFaceDir -= this.OnGetFaceDir;
        skillConfig.OnGetFaceDir += this.OnGetFaceDir;
        skillConfig.OwnFacePlayer -= this.OnFacePlayer;
        skillConfig.OwnFacePlayer += this.OnFacePlayer;

        tracks = skillConfig.GetTracks();
        foreach (var t in tracks)
        {
            if (t is SkillAnimationTrack)
            {
                t.Init(skillConfig, animator);
            }
            else if (t is SkillHitBoxTrack)
            {
                t.Init(skillConfig, OnHitBoxTriggered);
            }
            else if (t is SkillVelocityTrack)
            {
                t.Init(skillConfig, rb);
            }
            else
            {
                t.Init(skillConfig, null);
            }
        }
    }

    public async UniTask PlayAsync()
    {
        await PlayFromFrame(0);
    }

    public async UniTask PlayFromFrame(int startFrame)
    {
        if (skillConfig == null) return;

        IsPlaying = true;
        isPaused = false;
        currentFrame = Mathf.Clamp(startFrame, 0, skillConfig.FrameCount);
        frameElapsed = 0f;

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
                    
                    if (skillConfig.isLoopSkill)
                    {
                        IsPlaying = false;
                        this.skillConfig.SkillAttackTimeWindowData.OnSkillEnd();
                        await PlayFromFrame(0);
                    }
                    else
                    {
                        Stop();
                        OnSkillFinished?.Invoke();

                        if (!bufferedSkillName.IsNullOrWhitespace())
                        {
                            var nextSkill = await ResourceManager.LoadAssetAsync<SkillConfig>(ZString.Concat("Skill/", bufferedSkillName), ResType.ScriptObject);
                            bufferedSkillName = null;
                            SetSkill(nextSkill);
                            await PlayAsync();
                        }
                    }

                    return;
                }
            }

            await UniTask.WaitForFixedUpdate();
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
