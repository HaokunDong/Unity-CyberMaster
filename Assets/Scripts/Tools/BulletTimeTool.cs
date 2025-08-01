using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class BulletTimeTool
{
    private static CancellationTokenSource bulletTimeCTS;

    public static void PlayBulletTime(
        float durationRealtime,
        AnimationCurve curve,
        Vector2 timeScaleRange)
    {
        PlayBulletTime(durationRealtime, t => curve.Evaluate(t), timeScaleRange);
    }

    public static void PlayBulletTime(
        float durationRealtime,
        Func<float, float> scaleFunc01,
        Vector2 timeScaleRange)
    {
        bulletTimeCTS?.Cancel();
        bulletTimeCTS = new CancellationTokenSource();

        RunBulletTimeRoutine(durationRealtime, scaleFunc01, timeScaleRange, bulletTimeCTS.Token).Forget();
    }

    private static async UniTaskVoid RunBulletTimeRoutine(
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
                Time.timeScale = Mathf.Lerp(timeScaleRange.x, timeScaleRange.y, s);

                if (t >= 1f) break;

                await UniTask.Yield();
            }
        }
        catch (OperationCanceledException)
        {
            // ±»È¡Ïû£¬ºöÂÔ
        }
        finally
        {
            Time.timeScale = 1f;
        }
    }

    public static void Cancel()
    {
        bulletTimeCTS?.Cancel();
        bulletTimeCTS = null;
        Time.timeScale = 1f;
    }
}
