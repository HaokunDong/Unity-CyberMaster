using System;
using System.Collections.Generic;

public enum UpdateMode
{
    Update,
    FixedUpdate,
    LateUpdate
}

public class CooldownGroup
{
    public UpdateMode UpdateMode { get; private set; }
    private readonly Dictionary<string, Cooldown> _cooldowns = new();

    public CooldownGroup(UpdateMode updateMode)
    {
        UpdateMode = updateMode;
    }

    public Cooldown Set(string key, float duration, bool affectedByTimeScale = true)
    {
        if (!_cooldowns.TryGetValue(key, out var cd))
        {
            cd = new Cooldown(duration, affectedByTimeScale);
            _cooldowns[key] = cd;
        }
        cd.Reset(duration);
        return cd;
    }

    public void Reset(string key, float? duration = null)
    {
        if (_cooldowns.TryGetValue(key, out var cd))
        {
            cd.Reset(duration);
        }
    }

    public bool IsReady(string key)
    {
        return !_cooldowns.TryGetValue(key, out var cd) || cd.IsReady;
    }

    public void SetPaused(string key, bool paused)
    {
        if (_cooldowns.TryGetValue(key, out var cd))
        {
            cd.Paused = paused;
        }
    }

    public void SetTimeScale(string key, float scale)
    {
        if (_cooldowns.TryGetValue(key, out var cd))
        {
            cd.TimeScale = scale;
        }
    }

    public float GetRemaining(string key)
    {
        return _cooldowns.TryGetValue(key, out var cd) ? cd.Remaining : 0f;
    }

    public void Tick(float dt, float unscaledDt)
    {
        foreach (var cd in _cooldowns.Values)
        {
            float d = cd.AffectedByTimeScale ? dt : unscaledDt;
            cd.Tick(d);
        }
    }

    public void AddTickCallback(string key, string tag, Action<float> cb)
    {
        if (_cooldowns.TryGetValue(key, out var cd))
            cd.AddTickCallback(tag, cb);
    }

    public void AddCooldownEndCallback(string key, string tag, Action cb)
    {
        if (_cooldowns.TryGetValue(key, out var cd))
            cd.AddCooldownEndCallback(tag, cb);
    }

    public void RemoveCallbacksByTag(string key, string tag)
    {
        if (_cooldowns.TryGetValue(key, out var cd))
            cd.RemoveCallbacksByTag(tag);
    }

    public void ClearAllCallbacks(string key)
    {
        if (_cooldowns.TryGetValue(key, out var cd))
            cd.ClearAllCallbacks();
    }
}
