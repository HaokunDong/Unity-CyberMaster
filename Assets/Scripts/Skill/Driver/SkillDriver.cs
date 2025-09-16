using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameBase.Log;
using Managers;
using Sirenix.Utilities;
using UnityEngine;

public interface ISkillDriverUnit
{
    public GamePlayEntity skillDriverOwner { get; }
    public SkillDriver skillDriverImp { get; }
    public bool isOnGround { get; }
}

public class SkillDriver
{
    public readonly Func<float> getDeltaTime;
    private readonly Animator animator;
    private readonly Rigidbody2D rb;
    private GamePlayEntity owner;
    private CancellationTokenSource skillCTS;

    public SkillConfig skillConfig { get; set; } = null;
    public uint SkillOwnerGPId { get; private set; }
    public Type ShillOwnerGPType { get; private set; }
    public SkillAttributeData attributeData;

    private int currentFrame;
    public int CurrentFrame => currentFrame;

    private float frameElapsed;
    private float frameInterval => skillConfig ? (1f / skillConfig.FrameRate) : 0.1f;
    private float startTime = 0f;

    private bool isPlaying = false;
    private bool isPaused = false;
    private string bufferedSkillName = null;
    public string BufferedSkillName => bufferedSkillName;
    private int skillTailCutFrame = -1;

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

    public bool IsPlayingABlockSkill()
    {
        if (IsPlaying)
        {
            if(skillConfig != null && skillConfig.SkillBlockBoxData != null && skillConfig.SkillBlockBoxData.skillClipDict.Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    private HitResType hitResTp = HitResType.None;
    public HitResType HitResTp
    {
        get => hitResTp;
        private set
        {
            if (hitResTp != value)
            {
                hitResTp = value;
                RestHitResType().Forget();
            }
        }
    }

    private async UniTask RestHitResType()
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        hitResTp = HitResType.None;
    }


    public bool IsPaused => isPaused;

    public event Action OnSkillFinished;
    private event Func<int> OnGetFaceDir;
    public event Action<HitResType, uint, uint, float, Vector2> OnHitBoxTriggered;
    private event Action OnFacePlayer;

    private List<ISkillTrack> tracks;

    public SkillDriver(GamePlayEntity entity, Type type, Animator animator, Rigidbody2D rb, Action<HitResType, uint, uint, float, Vector2> OnHitBoxTriggered, Func<float> getDeltaTime, Func<int> getDir, Action facePlayer)
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

        attributeData = new SkillAttributeData(this);
    }

    public void BufferNextSkill(string skillName, int tailCutFrame)
    {
        bufferedSkillName = skillName;
        skillTailCutFrame = tailCutFrame;
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

    public async UniTask PlayAsync(bool resetVelocity)
    {
        skillTailCutFrame = -1;
        skillCTS?.Cancel(); // 先取消之前的
        skillCTS = new CancellationTokenSource();

        try
        {
            await PlayFromFrame(0, resetVelocity, skillCTS.Token);
        }
        catch (OperationCanceledException)
        {
            LogUtils.Trace("技能被取消", LogChannel.Battle, Color.cyan);
        }
    }

    public void JumpToFrame(int frame)
    {
        currentFrame = Mathf.Clamp(frame, 0, skillConfig.FrameCount);
    }

    public async UniTask CancelSkill(bool waitMinTime, bool stopInvoke)
    {
        if (!IsPlaying) return;

        if(waitMinTime)
        {
            await UniTask.WaitUntil(() => Time.realtimeSinceStartup - startTime > 0.1f);
        }

        skillCTS?.Cancel(); // 正确取消异步任务
        skillCTS?.Dispose();
        skillCTS = null;

        Stop();
        if(stopInvoke)
        {
            OnSkillFinished?.Invoke();
        }

        // 清除所有属性
        ClearAllAttributes();

        bufferedSkillName = null;
        skillConfig = null;
        IsPlaying = false;
    }

    public async UniTask ChangeSkillAsync(string newSkillPath, bool resetVelocity)
    {
        SkillConfig newSkill = null;
        if(!newSkillPath.StartsWith("Skill/"))
        {
            newSkillPath = ZString.Concat("Skill/", newSkillPath);
        }
        var newSkillTask = ResourceManager.LoadAssetAsync<SkillConfig>(newSkillPath, ResType.ScriptObject);
        var cancelTask = CancelSkill(false, false);
        await UniTask.WhenAll(
            newSkillTask.ContinueWith(result => newSkill = result),
            cancelTask
        );
        if(newSkill != null)
        {
            SetSkill(newSkill);
            await PlayAsync(resetVelocity);
        }
    }

    public async UniTask ChangeSkillAsync(SkillConfig newSkill, bool resetVelocity)
    {
        var cancelTask = CancelSkill(false, false);
        await UniTask.WhenAll(
            cancelTask
        );
        if (newSkill != null)
        {
            SetSkill(newSkill);
            await PlayAsync(resetVelocity);
        }
    }

    public async UniTask PlayFromFrame(int startFrame, bool resetVelocity, CancellationToken token)
    {
        if (skillConfig == null) return;
        if(resetVelocity && rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        IsPlaying = true;
        isPaused = false;
        currentFrame = Mathf.Clamp(startFrame, 0, skillConfig.FrameCount);
        frameElapsed = 0f;
        startTime = Time.realtimeSinceStartup;

        int maxFrame = skillConfig.FrameCount;

        while (IsPlaying && currentFrame <= maxFrame)
        {
            token.ThrowIfCancellationRequested();

            if (isPaused)
            {
                await UniTask.WaitForFixedUpdate();
                continue;
            }

            float delta = getDeltaTime?.Invoke() ?? Time.fixedDeltaTime;
            frameElapsed += delta;

            while (frameElapsed >= frameInterval)
            {
                if(skillTailCutFrame == currentFrame)
                {
                    currentFrame = maxFrame + 1;
                }
                else
                {
                    frameElapsed -= frameInterval;

                    foreach (var t in tracks)
                    {
                        t.Update(currentFrame);
                    }

                    // 更新属性管理器
                    attributeData.UpdateAttributes();

                    currentFrame++;
                }
                
                if (currentFrame > maxFrame)
                {

                    Stop();
                    

                    if (!bufferedSkillName.IsNullOrWhitespace())
                    {
                        var nextSkill = await ResourceManager.LoadAssetAsync<SkillConfig>(ZString.Concat("Skill/", bufferedSkillName), ResType.ScriptObject);
                        bufferedSkillName = null;
                        SetSkill(nextSkill);
                        await PlayAsync(resetVelocity);
                    }
                    else
                    {
                        OnSkillFinished?.Invoke();
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

    /// <summary>
    /// 添加技能属性
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <param name="value">属性值</param>
    /// <param name="addType">添加方式</param>
    /// <param name="duration">持续时间（帧数或秒数，取决于添加方式）</param>
    public void AddAttribute(AddSkillAttribute aa)
    {
        attributeData.AddAttribute(aa);
    }

    /// <summary>
    /// 移除指定类型的所有属性
    /// </summary>
    /// <param name="type">属性类型</param>
    public void RemoveAllAttributes(GamePlayAttributeType type)
    {
        attributeData.RemoveAllAttributes(type);
    }

    /// <summary>
    /// 获取指定类型的属性总值
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <returns>属性总值</returns>
    public float GetAttributeValue(GamePlayAttributeType type)
    {
        return attributeData.GetAttributeValue(type);
    }

    /// <summary>
    /// 检查是否有指定类型的激活属性
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <returns>是否有激活的属性</returns>
    public bool HasActiveAttribute(GamePlayAttributeType type)
    {
        return attributeData.HasActiveAttribute(type);
    }

    /// <summary>
    /// 清除所有属性
    /// </summary>
    public void ClearAllAttributes()
    {
        attributeData.ClearAllAttributes();
    }

    public void Stop()
    {
        foreach (var t in tracks)
        {
            t.OnSkillEnd();
        }

        IsPlaying = false;
        animator.speed = 1f;
    }

    public void OnHit(HitResType hitRestype, uint attackerGPId, uint beHitterGPId, float damageBaseValue, Vector2 hitPoint)
    {
        OnHitBoxTriggered?.Invoke(hitRestype, attackerGPId, beHitterGPId, damageBaseValue, hitPoint);
        HitResTp = hitRestype;
    }
}
