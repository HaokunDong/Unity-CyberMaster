using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 轻量级特效驱动器 - 专注于播放控制和轨道管理
/// </summary>
public class EffectDriver
{
    private readonly Func<float> getDeltaTime;
    private readonly EffectInstance effectInstance;
    private CancellationTokenSource effectCTS;

    public EffectConfig Config { get; private set; }

    private int currentFrame;
    private float frameElapsed;
    private float frameInterval => Config ? (1f / Config.frameRate) : 0.1f;

    private bool isPlaying = false;
    private bool isPaused = false;

    // 简化的轨道系统
    private List<IEffectTrack> tracks;

    public bool IsPlaying => isPlaying;
    public int CurrentFrame => currentFrame;
    public bool IsPaused => isPaused;

    public event Action OnEffectFinished;

    public EffectDriver(EffectInstance instance, Func<float> getDeltaTime)
    {
        this.effectInstance = instance;
        this.getDeltaTime = getDeltaTime ?? (() => Time.fixedDeltaTime);
    }

    public void SetConfig(EffectConfig config)
    {
        Config = config;
        InitializeTracks();
    }

    private void InitializeTracks()
    {
        tracks = new List<IEffectTrack>();

        // 只添加需要帧控制的轨道
        if (Config.movementConfig.enableMovement)
        {
            tracks.Add(new EffectMovementTrack());
        }

        if (Config.scaleConfig.enableScale)
        {
            tracks.Add(new EffectScaleTrack());
        }

        if (Config.audioConfig.playAudio)
        {
            tracks.Add(new EffectAudioTrack());
        }

        // 特效类型特定的轨道
        switch (Config.effectType)
        {
            case EffectType.SequenceFrame:
                tracks.Add(new SequenceFrameTrack());
                break;
            case EffectType.FrameAnimation:
                tracks.Add(new FrameAnimationTrack());
                break;
        }

        foreach (var track in tracks)
        {
            track.Initialize(Config, effectInstance);
        }
    }

    public async UniTask PlayAsync()
    {
        effectCTS?.Cancel();
        effectCTS = new CancellationTokenSource();

        try
        {
            await PlayFromFrame(0, effectCTS.Token);
        }
        catch (OperationCanceledException)
        {
            // 正常取消，不输出日志
        }
    }

    private async UniTask PlayFromFrame(int startFrame, CancellationToken token)
    {
        if (Config == null) return;

        isPlaying = true;
        isPaused = false;
        currentFrame = Mathf.Clamp(startFrame, 0, Config.durationFrames);
        frameElapsed = 0f;

        // 启动所有轨道
        foreach (var track in tracks)
        {
            track.OnEffectStart();
        }

        while (isPlaying && ShouldContinuePlaying())
        {
            token.ThrowIfCancellationRequested();

            if (isPaused)
            {
                await UniTask.WaitForFixedUpdate();
                continue;
            }

            float delta = getDeltaTime();
            frameElapsed += delta;

            while (frameElapsed >= frameInterval)
            {
                frameElapsed -= frameInterval;

                // 更新所有轨道
                foreach (var track in tracks)
                {
                    track.UpdateFrame(currentFrame);
                }

                currentFrame++;

                if (!ShouldContinuePlaying())
                {
                    break;
                }
            }

            await UniTask.WaitForFixedUpdate();
        }

        await Stop();
    }

    private bool ShouldContinuePlaying()
    {
        switch (Config.playMode)
        {
            case EffectPlayMode.Once:
                return currentFrame < Config.durationFrames;
            case EffectPlayMode.Loop:
                if (currentFrame >= Config.durationFrames)
                {
                    currentFrame = 0; // 重置循环
                }
                return true;
            case EffectPlayMode.Manual:
                return true;
            default:
                return currentFrame < Config.durationFrames;
        }
    }

    public async UniTask Stop()
    {
        if (!isPlaying) return;

        isPlaying = false;

        foreach (var track in tracks)
        {
            track.OnEffectEnd();
        }

        effectCTS?.Cancel();
        effectCTS?.Dispose();
        effectCTS = null;

        OnEffectFinished?.Invoke();
    }

    public void Pause()
    {
        isPaused = true;
        foreach (var track in tracks)
        {
            track.OnEffectPause();
        }
    }

    public void Resume()
    {
        isPaused = false;
        foreach (var track in tracks)
        {
            track.OnEffectResume();
        }
    }

    public void JumpToFrame(int frame)
    {
        currentFrame = Mathf.Clamp(frame, 0, Config.durationFrames);
        foreach (var track in tracks)
        {
            track.UpdateFrame(currentFrame);
        }
    }

    public float GetProgress()
    {
        if (Config == null || Config.durationFrames <= 0)
            return 0f;
        return (float)currentFrame / Config.durationFrames;
    }

    public void Dispose()
    {
        effectCTS?.Cancel();
        effectCTS?.Dispose();
        effectCTS = null;

        foreach (var track in tracks)
        {
            track?.Dispose();
        }
        tracks?.Clear();
    }
}

/// <summary>
/// 序列帧轨道
/// </summary>
public class SequenceFrameTrack : IEffectTrack
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
        instance.UpdateSequenceFrame(frame);
    }

    public void OnEffectEnd() { }
    public void OnEffectPause() { }
    public void OnEffectResume() { }
    public void Dispose() { }
}

/// <summary>
/// 帧动画轨道
/// </summary>
public class FrameAnimationTrack : IEffectTrack
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
        instance.UpdateFrameAnimation(frame);
    }

    public void OnEffectEnd() { }
    public void OnEffectPause() { }
    public void OnEffectResume() { }
    public void Dispose() { }
}
