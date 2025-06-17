using System;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 支持异步执行、间隔循环、初始延迟、最大执行次数、取消、中断条件、暂停、完成回调的任务工具类。
/// </summary>
public class RepeatingTask : IDisposable
{
    private readonly float interval;
    private readonly float initialDelay;
    private readonly Func<UniTask> action;
    private readonly int? maxRunCount;
    private readonly Func<bool> continueCondition;
    private readonly Action onCompleted;

    private CancellationTokenSource internalCts;
    private CancellationTokenSource linkedCts;
    private bool isRunning;
    private bool isPaused;
    private int runCounter;

    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;

    public RepeatingTask(
        float intervalInSeconds,
        Func<UniTask> action,
        CancellationToken externalToken = default,
        float initialDelayInSeconds = 0f,
        int? maxRunCount = null,
        Func<bool> continueCondition = null,
        Action onCompleted = null
    )
    {
        this.interval = intervalInSeconds;
        this.initialDelay = initialDelayInSeconds;
        this.action = action;
        this.maxRunCount = maxRunCount;
        this.continueCondition = continueCondition;
        this.onCompleted = onCompleted;

        internalCts = new CancellationTokenSource();

        linkedCts = externalToken != default
            ? CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, externalToken)
            : CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token);
    }

    public void Start()
    {
        if (isRunning) return;
        isRunning = true;
        isPaused = false;
        runCounter = 0;
        LoopAsync().Forget();
    }

    public void Pause() => isPaused = true;
    public void Resume()
    {
        if (isRunning && isPaused)
            isPaused = false;
    }

    public void Stop()
    {
        if (!isRunning) return;
        internalCts.Cancel();
        isRunning = false;
    }

    public void Dispose()
    {
        Stop();
        internalCts?.Dispose();
        linkedCts?.Dispose();
    }

    private async UniTaskVoid LoopAsync()
    {
        try
        {
            if (initialDelay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(initialDelay), cancellationToken: linkedCts.Token);

            while (!linkedCts.Token.IsCancellationRequested)
            {
                if (isPaused)
                {
                    await UniTask.Delay(100, cancellationToken: linkedCts.Token);
                    continue;
                }

                if (maxRunCount.HasValue && runCounter >= maxRunCount.Value)
                    break;

                if (continueCondition != null && !continueCondition.Invoke())
                    break;

                await action.Invoke();
                runCounter++;

                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: linkedCts.Token);
            }

            onCompleted?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // 安全取消
        }
    }
}
