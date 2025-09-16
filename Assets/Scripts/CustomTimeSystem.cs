using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 时间分组
/// </summary>
public enum TimeGroup
{
    Default,
    Player,
    Enemy,
    UI,
    VFX
}

/// <summary>
/// 全局自定义时间系统
/// 支持分组、单位注册、子弹时间、权重优先
/// </summary>
public static class CustomTimeSystem
{
    #region 内部数据结构

    private class GroupData
    {
        public float TimeScale = 1f;             // 当前组缩放
        public float DeltaTime = 0f;             // 当前组 deltaTime
        public float FixedDeltaTime = 0f;        // 当前组 fixedDeltaTime
        public HashSet<GameObject> RegisteredUnits = new HashSet<GameObject>();

        // 当前生效的子弹时间请求
        public BulletTimeRequest ActiveBulletTime;
    }

    private class BulletTimeRequest
    {
        public int Weight;
        public CancellationTokenSource CTS;
    }

    private static readonly Dictionary<TimeGroup, GroupData> groups = new();

    static CustomTimeSystem()
    {
        foreach (TimeGroup g in Enum.GetValues(typeof(TimeGroup)))
        {
            groups[g] = new GroupData { TimeScale = 1f };
        }
    }

    #endregion

    #region 全局更新

    /// <summary>
    /// 每帧更新各组 deltaTime（必须在全局 Update 中调用一次）
    /// </summary>
    public static void Update()
    {
        float unscaledDt = Time.unscaledDeltaTime;
        float fixedUnscaledDt = Time.fixedUnscaledDeltaTime;

        foreach (var kv in groups)
        {
            var g = kv.Value;
            g.DeltaTime = unscaledDt * g.TimeScale;
            g.FixedDeltaTime = fixedUnscaledDt * g.TimeScale;
        }
    }

    #endregion

    #region 单位注册管理

    /// <summary>
    /// 注册单位到指定组（自动处理 Animator / ParticleSystem）
    /// </summary>
    public static void RegisterUnit(GameObject unit, TimeGroup group)
    {
        if (unit == null) return;

        groups[group].RegisteredUnits.Add(unit);

        var anim = unit.GetComponentInChildren<Animator>();
        if (anim) anim.updateMode = AnimatorUpdateMode.UnscaledTime;

        foreach (var ps in unit.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.useUnscaledTime = true;
        }
    }

    /// <summary>
    /// 取消注册单位
    /// </summary>
    public static void UnregisterUnit(GameObject unit, TimeGroup group)
    {
        if (unit == null) return;

        groups[group].RegisteredUnits.Remove(unit);

        var anim = unit.GetComponentInChildren<Animator>();
        if (anim) anim.updateMode = AnimatorUpdateMode.Normal;

        foreach (var ps in unit.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.useUnscaledTime = false;
        }
    }

    /// <summary>
    /// 查询单位是否注册在某组
    /// </summary>
    public static bool IsUnitRegistered(GameObject unit, TimeGroup group)
    {
        return unit != null && groups[group].RegisteredUnits.Contains(unit);
    }

    #endregion

    #region DeltaTime 获取

    public static float DeltaTimeOf(TimeGroup group) => groups[group].DeltaTime;
    public static float FixedDeltaTimeOf(TimeGroup group) => groups[group].FixedDeltaTime;

    #endregion

    #region TimeScale 操作

    public static void SetTimeScale(TimeGroup group, float scale)
    {
        groups[group].TimeScale = Mathf.Max(0f, scale);
    }

    #endregion

    #region 子弹时间（权重优先）兼容原接口

    /// <summary>
    /// 播放子弹时间
    /// </summary>
    public static void PlayBulletTime(
        int weight,
        TimeGroup group,
        float durationRealtime,
        AnimationCurve curve,
        Vector2 timeScaleRange)
    {
        PlayBulletTime(weight, group, durationRealtime, t => curve.Evaluate(t), timeScaleRange);
    }

    public static void PlayBulletTime(
        int weight,
        TimeGroup group,
        float durationRealtime,
        Func<float, float> scaleFunc01,
        Vector2 timeScaleRange)
    {
        var g = groups[group];

        if (g.ActiveBulletTime == null || weight >= g.ActiveBulletTime.Weight)
        {
            g.ActiveBulletTime?.CTS.Cancel();
            var req = new BulletTimeRequest { Weight = weight, CTS = new CancellationTokenSource() };
            g.ActiveBulletTime = req;

            RunBulletTimeRoutine(group, req, durationRealtime, scaleFunc01, timeScaleRange, req.CTS.Token).Forget();
        }
    }

    private static async UniTaskVoid RunBulletTimeRoutine(
        TimeGroup group,
        BulletTimeRequest req,
        float durationRealtime,
        Func<float, float> scaleFunc01,
        Vector2 timeScaleRange,
        CancellationToken token)
    {
        try
        {
            float startTime = Time.realtimeSinceStartup;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                float elapsed = Time.realtimeSinceStartup - startTime;
                float t = Mathf.Clamp01(elapsed / durationRealtime);
                float s = scaleFunc01(t);

                // 设置组的 timeScale
                SetTimeScale(group, Mathf.Lerp(timeScaleRange.x, timeScaleRange.y, s));

                if (t >= 1f) break;

                await UniTask.Yield();
            }
        }
        catch (OperationCanceledException)
        {
            // 忽略
        }
        finally
        {
            SetTimeScale(group, 1f);
            req.Weight = 0;
            req.CTS = null;
            groups[group].ActiveBulletTime = null;
        }
    }

    /// <summary>
    /// 取消子弹时间
    /// </summary>
    public static void CancelBulletTime(TimeGroup group)
    {
        var g = groups[group];
        g.ActiveBulletTime?.CTS.Cancel();
        g.ActiveBulletTime = null;
        SetTimeScale(group, 1f);
    }

    #endregion
}
