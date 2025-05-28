using GameBase.Log;
using System;
using System.Collections.Generic;

public class Cooldown
{
    public bool AffectedByTimeScale { get; private set; }
    public bool Paused { get; set; } = false;
    public float TimeScale { get; set; } = 1f;
    public float Remaining => _remaining <= 0f ? 0f : _remaining;
    public bool IsReady => _remaining <= 0f;
    private float _remaining;

    private List<(string tag, Action cb)> _onCooldownEnd = new();
    private List<(string tag, Action<float> cb)> _onTick = new();

    public Cooldown(float duration, bool affectedByTimeScale)
    {
        AffectedByTimeScale = affectedByTimeScale;
        Reset(duration, clearCallbacks: false);
    }

    public void Reset(float? newDuration = null, bool clearCallbacks = false)
    {
        if (newDuration.HasValue)
            _remaining = newDuration.Value;
        if (clearCallbacks)
        {
            _onCooldownEnd.Clear();
            _onTick.Clear();
        }
    }

    public void Tick(float dt)
    {
        if (Paused || _remaining <= 0f) return;

        float prev = _remaining;
        _remaining -= dt * TimeScale;

        foreach (var (tag, cb) in _onTick)
        {
            try { cb?.Invoke(Math.Max(_remaining, 0f)); }
            catch (Exception e)
            {
                LogUtils.Error($"CD Tick Callback error: {e}");
            }
        }

        if (prev > 0f && _remaining <= 0f)
        {
            _remaining = 0f;
            OnCooldownEnd();
        }
    }

    private void OnCooldownEnd()
    {
        foreach (var (tag, cb) in _onCooldownEnd)
        {
            try { cb?.Invoke(); }
            catch (Exception e)
            {
                LogUtils.Error($"CD End Callback error: {e}");
            }
        }
    }

    public void AddCooldownEndCallback(string tag, Action callback)
    {
        if (callback != null)
            _onCooldownEnd.Add((tag, callback));
    }

    public void AddTickCallback(string tag, Action<float> callback)
    {
        if (callback != null)
            _onTick.Add((tag, callback));
    }

    public void RemoveCallbacksByTag(string tag)
    {
        _onCooldownEnd.RemoveAll(e => e.tag == tag);
        _onTick.RemoveAll(e => e.tag == tag);
    }

    public void ClearAllCallbacks()
    {
        _onCooldownEnd.Clear();
        _onTick.Clear();
    }
}
