using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class BulletTimeTool
{
    private static CancellationTokenSource bulletTimeCTS;
    private static int currentWeight = 0;

    public static void PlayBulletTime(
        int weight,
        float durationRealtime,
        AnimationCurve curve,
        Vector2 timeScaleRange)
    {
        PlayBulletTime(weight, durationRealtime, t => curve.Evaluate(t), timeScaleRange);
    }

    public static void PlayBulletTime(
        int weight,
        float durationRealtime,
        Func<float, float> scaleFunc01,
        Vector2 timeScaleRange)
    {
        if(weight >= currentWeight)
        {
            currentWeight = weight;
            bulletTimeCTS?.Cancel();
            bulletTimeCTS = new CancellationTokenSource();

            RunBulletTimeRoutine(weight, durationRealtime, scaleFunc01, timeScaleRange, bulletTimeCTS.Token).Forget();
        }
    }

    private static async UniTaskVoid RunBulletTimeRoutine(
        int weight,
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
