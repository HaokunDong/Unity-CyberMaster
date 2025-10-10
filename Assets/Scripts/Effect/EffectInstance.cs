using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// 特效实例
/// </summary>
public class EffectInstance : MonoBehaviour, ICustomHierarchyComment
{
    [ReadOnly] public uint effectId;
    [ReadOnly] public EffectConfig config;
    [ReadOnly] public EffectAttachType attachType;
    [ReadOnly] public Transform followTarget;
    [ReadOnly] public Vector3 attachOffset;

    private EffectDriver driver;
    private bool isInitialized = false;
    private bool isPlaying = false;

    // 组件缓存
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private ParticleSystem[] particleSystems;
    private List<EffectInstance> subEffectInstances;

    // 用于跟随目标的缓存
    private Vector3 lastFollowPosition;
    private bool hasValidFollowTarget;

    public bool IsPlaying => isPlaying;
    public bool IsInitialized => isInitialized;
    public int CurrentFrame => driver?.CurrentFrame ?? 0;
    public EffectDriver Driver => driver;

    /// <summary>
    /// 初始化特效实例
    /// </summary>
    public void Initialize(EffectConfig effectConfig, EffectDriver effectDriver = null)
    {
        if (isInitialized)
        {
            Debug.LogWarning($"EffectInstance {effectId} is already initialized");
            return;
        }

        config = effectConfig;
        effectId = EffectManager.Ins.GenerateEffectId();

        // 创建或使用传入的驱动器
        if (effectDriver != null)
        {
            driver = effectDriver;
        }
        else
        {
            // 修正：使用正确的构造函数参数
            driver = new EffectDriver(
                this,
                () => CustomTimeSystem.FixedDeltaTimeOf(config.timeGroup)
            );
        }

        // 修正：使用正确的方法名
        driver.SetConfig(config);
        driver.OnEffectFinished += OnEffectFinished;

        SetupComponents();
        SetupSortingLayer();
        CustomTimeSystem.RegisterUnit(gameObject, config.timeGroup);

        isInitialized = true;
    }

    /// <summary>
    /// 设置组件
    /// </summary>
    private void SetupComponents()
    {
        switch (config.effectType)
        {
            case EffectType.FrameAnimation:
                SetupFrameAnimation();
                break;
            case EffectType.Particle:
                SetupParticle();
                break;
            case EffectType.SequenceFrame:
                SetupSequenceFrame();
                break;
            case EffectType.Composite:
                SetupComposite().Forget();
                break;
        }
    }

    private void SetupFrameAnimation()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        // 移除编辑器专用代码，改为运行时创建
        if (config.animationClip != null)
        {
            // 在运行时，我们可以直接播放动画剪辑
            // 不需要创建 AnimatorController
        }
    }

    private void SetupParticle()
    {
        // 如果已有粒子系统组件，直接使用
        particleSystems = GetComponentsInChildren<ParticleSystem>();

        // 如果没有且配置了粒子预制体，实例化
        if (particleSystems.Length == 0 && config.particlePrefab != null)
        {
            var particleGO = Instantiate(config.particlePrefab, transform);
            particleSystems = particleGO.GetComponentsInChildren<ParticleSystem>();
        }

        // 设置粒子系统使用非缩放时间
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            main.useUnscaledTime = true;
        }
    }

    private void SetupSequenceFrame()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // 设置初始精灵
        if (config.frameSprites != null && config.frameSprites.Length > 0)
        {
            spriteRenderer.sprite = config.frameSprites[0];
        }
    }

    private async UniTask SetupComposite()
    {
        if (config.subEffects == null || config.subEffects.Length == 0)
            return;

        subEffectInstances = new List<EffectInstance>();

        for (int i = 0; i < config.subEffects.Length; i++)
        {
            var subConfig = config.subEffects[i];
            if (subConfig.effectConfig == null) continue;

            try
            {
                var subInstance = await EffectManager.Ins.CreateEffect(subConfig.effectConfig, transform);
                subInstance.transform.localPosition = subConfig.relativePosition;
                subInstance.transform.localEulerAngles = subConfig.relativeRotation;
                subInstance.transform.localScale = subConfig.relativeScale;
                subEffectInstances.Add(subInstance);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create sub effect {i}: {e.Message}");
            }
        }
    }

    private void SetupSortingLayer()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            renderer.sortingLayerName = config.sortingLayerName;
            renderer.sortingOrder = config.sortingOrder;
        }
    }

    /// <summary>
    /// 播放特效
    /// </summary>
    public async UniTask PlayAsync()
    {
        if (!isInitialized)
        {
            Debug.LogError("EffectInstance is not initialized");
            return;
        }

        if (isPlaying)
        {
            Debug.LogWarning($"EffectInstance {effectId} is already playing");
            return;
        }

        isPlaying = true;

        try
        {
            // 启动复合特效的子特效
            if (config.effectType == EffectType.Composite && subEffectInstances != null)
            {
                PlayCompositeEffects();
            }

            // 启动粒子系统
            if (particleSystems != null)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps != null) ps.Play();
                }
            }

            // 播放动画 - 简化版本
            if (animator != null && config.animationClip != null)
            {
                // 直接播放动画剪辑，如果有RuntimeAnimatorController的话
                if (animator.runtimeAnimatorController != null)
                {
                    animator.Play(config.animationClip.name, 0, 0f);
                }
            }

            // 启动驱动器
            if (driver != null)
            {
                await driver.PlayAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error playing effect {effectId}: {e.Message}");
            isPlaying = false;
            throw;
        }
    }

    /// <summary>
    /// 播放复合特效
    /// </summary>
    private void PlayCompositeEffects()
    {
        if (subEffectInstances == null) return;

        for (int i = 0; i < subEffectInstances.Count && i < config.subEffects.Length; i++)
        {
            var subInstance = subEffectInstances[i];
            var subConfig = config.subEffects[i];

            if (subInstance == null) continue;

            if (subConfig.delayFrames > 0)
            {
                DelayedPlaySubEffect(subInstance, subConfig.delayFrames).Forget();
            }
            else
            {
                subInstance.PlayAsync().Forget();
            }
        }
    }

    private async UniTaskVoid DelayedPlaySubEffect(EffectInstance subInstance, int delayFrames)
    {
        float delayTime = delayFrames / (float)config.frameRate;
        await UniTask.Delay(TimeSpan.FromSeconds(delayTime), true);

        if (subInstance != null && isPlaying)
        {
            await subInstance.PlayAsync();
        }
    }

    /// <summary>
    /// 停止特效
    /// </summary>
    public async UniTask Stop()
    {
        if (!isPlaying) return;

        isPlaying = false;

        try
        {
            // 停止驱动器
            if (driver != null)
            {
                await driver.Stop();
            }

            // 停止粒子系统
            if (particleSystems != null)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps != null) ps.Stop();
                }
            }

            // 停止子特效
            if (subEffectInstances != null)
            {
                var stopTasks = new List<UniTask>();
                foreach (var subEffect in subEffectInstances)
                {
                    if (subEffect != null)
                    {
                        stopTasks.Add(subEffect.Stop());
                    }
                }
                if (stopTasks.Count > 0)
                {
                    await UniTask.WhenAll(stopTasks);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error stopping effect {effectId}: {e.Message}");
        }
        finally
        {
            OnEffectFinished();
        }
    }

    public void Pause()
    {
        driver?.Pause();

        if (particleSystems != null)
        {
            foreach (var ps in particleSystems)
            {
                if (ps != null) ps.Pause();
            }
        }

        if (subEffectInstances != null)
        {
            foreach (var subEffect in subEffectInstances)
            {
                subEffect?.Pause();
            }
        }
    }

    public void Resume()
    {
        driver?.Resume();

        if (particleSystems != null)
        {
            foreach (var ps in particleSystems)
            {
                if (ps != null) ps.Play();
            }
        }

        if (subEffectInstances != null)
        {
            foreach (var subEffect in subEffectInstances)
            {
                subEffect?.Resume();
            }
        }
    }

    /// <summary>
    /// 设置附着信息
    /// </summary>
    public void SetAttachment(EffectAttachType type, Transform target = null, Vector3 offset = default)
    {
        attachType = type;
        followTarget = target;
        attachOffset = offset;

        hasValidFollowTarget = followTarget != null;
        if (hasValidFollowTarget)
        {
            lastFollowPosition = followTarget.position;
        }
    }

    /// <summary>
    /// 更新序列帧
    /// </summary>
    public void UpdateSequenceFrame(int frame)
    {
        if (config.effectType != EffectType.SequenceFrame ||
            spriteRenderer == null ||
            config.frameSprites == null ||
            config.frameSprites.Length == 0)
            return;

        int spriteIndex = frame % config.frameSprites.Length;
        if (spriteIndex < config.frameSprites.Length && config.frameSprites[spriteIndex] != null)
        {
            spriteRenderer.sprite = config.frameSprites[spriteIndex];
        }
    }

    /// <summary>
    /// 更新动画
    /// </summary>
    public void UpdateFrameAnimation(int frame)
    {
        if (config.effectType != EffectType.FrameAnimation ||
            animator == null ||
            config.animationClip == null)
            return;

        float normalizedTime = (float)frame / config.durationFrames;
        normalizedTime = Mathf.Clamp01(normalizedTime);

        // 简化的动画播放，不需要手动控制速度
        if (animator.runtimeAnimatorController != null)
        {
            animator.Play(config.animationClip.name, 0, normalizedTime);
            animator.speed = 0f; // 手动控制播放进度
        }
    }

    private void Update()
    {
        UpdateAttachment();
    }

    /// <summary>
    /// 更新附着逻辑
    /// </summary>
    private void UpdateAttachment()
    {
        if (!isPlaying) return;

        switch (attachType)
        {
            case EffectAttachType.FollowTarget:
                UpdateFollowTarget();
                break;
            case EffectAttachType.AttachToBone:
                UpdateAttachToBone();
                break;
        }
    }

    private void UpdateFollowTarget()
    {
        if (!hasValidFollowTarget || followTarget == null)
        {
            hasValidFollowTarget = false;
            return;
        }

        Vector3 targetPosition = followTarget.position + attachOffset;
        transform.position = targetPosition;
        lastFollowPosition = targetPosition;
    }

    private void UpdateAttachToBone()
    {
        // 骨骼附着逻辑，可以根据需要扩展
        UpdateFollowTarget();
    }

    /// <summary>
    /// 特效结束回调
    /// </summary>
    private void OnEffectFinished()
    {
        isPlaying = false;

        if (isInitialized)
        {
            CustomTimeSystem.UnregisterUnit(gameObject, config.timeGroup);
        }

        // 清理驱动器事件
        if (driver != null)
        {
            driver.OnEffectFinished -= OnEffectFinished;
        }

        EffectManager.Ins.ReturnToPool(this);
    }

    /// <summary>
    /// 重置实例状态（用于对象池）
    /// </summary>
    public void ResetForPool()
    {
        isPlaying = false;
        isInitialized = false;
        config = null;
        effectId = 0;
        attachType = EffectAttachType.World;
        followTarget = null;
        attachOffset = Vector3.zero;
        hasValidFollowTarget = false;

        // 清理驱动器
        if (driver != null)
        {
            driver.OnEffectFinished -= OnEffectFinished;
            driver.Dispose();
            driver = null;
        }

        // 清理子特效
        if (subEffectInstances != null)
        {
            foreach (var subEffect in subEffectInstances)
            {
                if (subEffect != null)
                {
                    Destroy(subEffect.gameObject);
                }
            }
            subEffectInstances.Clear();
            subEffectInstances = null;
        }

        // 清理组件引用
        spriteRenderer = null;
        animator = null;
        particleSystems = null;
    }

    private void OnDestroy()
    {
        if (isInitialized)
        {
            CustomTimeSystem.UnregisterUnit(gameObject, config.timeGroup);
        }

        if (driver != null)
        {
            driver.OnEffectFinished -= OnEffectFinished;
            driver.Dispose();
        }
    }

#if UNITY_EDITOR
    public bool GetHierarchyComment(out string name, out Color color)
    {
        name = $"特效:{config?.effectName ?? "Unknown"} [{effectId}]";
        color = isPlaying ? Color.green : Color.magenta;
        return true;
    }
#endif
}
