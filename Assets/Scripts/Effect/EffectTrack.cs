using System;
using UnityEngine;

/// <summary>
/// 特效轨道接口
/// </summary>
public interface IEffectTrack : IDisposable
{
    void Initialize(EffectConfig config, EffectInstance instance);
    void OnEffectStart();
    void UpdateFrame(int frame);
    void OnEffectEnd();
    void OnEffectPause();
    void OnEffectResume();
}

/// <summary>
/// 特效移动轨道
/// </summary>
public class EffectMovementTrack : IEffectTrack
{
    private EffectConfig config;
    private EffectInstance instance;
    private Vector3 startPosition;

    public void Initialize(EffectConfig config, EffectInstance instance)
    {
        this.config = config;
        this.instance = instance;
    }

    public void OnEffectStart()
    {
        startPosition = instance.transform.position;
    }

    public void UpdateFrame(int frame)
    {
        if (!config.movementConfig.enableMovement) return;

        float t = (float)frame / config.durationFrames;
        float moveValue = config.movementConfig.movementCurve.Evaluate(t);
        Vector3 offset = config.movementConfig.moveDirection * config.movementConfig.moveDistance * moveValue;

        instance.transform.position = startPosition + offset;
    }

    public void OnEffectEnd() { }
    public void OnEffectPause() { }
    public void OnEffectResume() { }
    public void Dispose() { }
}

/// <summary>
/// 特效缩放轨道
/// </summary>
public class EffectScaleTrack : IEffectTrack
{
    private EffectConfig config;
    private EffectInstance instance;

    public void Initialize(EffectConfig config, EffectInstance instance)
    {
        this.config = config;
        this.instance = instance;
    }

    public void OnEffectStart() { }

    public void UpdateFrame(int frame)
    {
        if (!config.scaleConfig.enableScale) return;

        float t = (float)frame / config.durationFrames;
        float scaleValue = config.scaleConfig.scaleCurve.Evaluate(t);
        Vector3 scale = Vector3.Lerp(config.scaleConfig.startScale, config.scaleConfig.endScale, scaleValue);

        instance.transform.localScale = scale;
    }

    public void OnEffectEnd() { }
    public void OnEffectPause() { }
    public void OnEffectResume() { }
    public void Dispose() { }
}

/// <summary>
/// 特效音频轨道
/// </summary>
public class EffectAudioTrack : IEffectTrack
{
    private EffectConfig config;
    private EffectInstance instance;
    private bool audioPlayed = false;

    public void Initialize(EffectConfig config, EffectInstance instance)
    {
        this.config = config;
        this.instance = instance;
    }

    public void OnEffectStart()
    {
        audioPlayed = false;
    }

    public void UpdateFrame(int frame)
    {
        if (!config.audioConfig.playAudio || audioPlayed) return;

        if (frame >= config.audioConfig.delayFrames)
        {
            AkSoundEngine.PostEvent(config.audioConfig.audioEventName, instance.gameObject);
            audioPlayed = true;
        }
    }

    public void OnEffectEnd() { }
    public void OnEffectPause() { }
    public void OnEffectResume() { }
    public void Dispose() { }
}
