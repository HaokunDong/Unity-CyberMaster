using UnityEngine;

public class SmoothDirectionInput
{
    public float CurrentValue { get; private set; }

    public float MaxValue = 1f;
    public float MinValue = -1f;

    private float acceleration;
    private float deceleration;
    private float rawInput;
    private bool isAnalog;

    private float minStartValue;
    private float deadThreshold;

    private bool useCurve;
    private AnimationCurve accelerationCurve;

    private bool enableEaseFunction;
    private System.Func<float, float> easeFunc;

    private bool enableTurnBoost;
    private float turnBoostMultiplier;

    public SmoothDirectionInput(
        float acceleration,
        float deceleration,
        float minStartValue = 0.1f,
        float deadThreshold = 0.05f,
        bool enableEase = false,
        System.Func<float, float> easeFunc = null,
        bool enableTurnBoost = true,
        float turnBoostMultiplier = float.MaxValue)
    {
        this.acceleration = acceleration;
        this.deceleration = deceleration;
        this.minStartValue = minStartValue;
        this.deadThreshold = deadThreshold;
        this.enableEaseFunction = enableEase;
        this.easeFunc = easeFunc ?? EaseOutQuad;
        this.enableTurnBoost = enableTurnBoost;
        this.turnBoostMultiplier = turnBoostMultiplier;
    }

    public void UseCurve(AnimationCurve curve)
    {
        this.accelerationCurve = curve;
        this.useCurve = curve != null;
    }

    public void SetMinAndMax(float minValue, float maxValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);
    }

    public void SetRawInput(float value)
    {
        rawInput = value;
        isAnalog = Mathf.Abs(value) > 0f && Mathf.Abs(value) < 0.99f;
    }

    public void Update(float deltaTime)
    {
        if (isAnalog)
        {
            CurrentValue = Mathf.Abs(rawInput) < deadThreshold ? 0f : rawInput;
            if (Mathf.Abs(CurrentValue) < minStartValue)
                CurrentValue = minStartValue * Mathf.Sign(rawInput);

            CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);
            return;
        }

        // --- 非模拟输入逻辑 ---
        if (Mathf.Abs(rawInput) > 0.01f)
        {
            float delta = rawInput - CurrentValue;

            // 起步直接拉起
            if (Mathf.Approximately(CurrentValue, 0f))
                CurrentValue = minStartValue * Mathf.Sign(rawInput);

            float boost = 1f;

            // 转向加速提升
            if (enableTurnBoost && Mathf.Sign(rawInput) != Mathf.Sign(CurrentValue) && Mathf.Abs(CurrentValue) > 0.01f)
                boost = turnBoostMultiplier;

            if (useCurve)
            {
                float t = Mathf.Clamp01(Mathf.Abs(delta));
                float curveFactor = accelerationCurve.Evaluate(t);
                CurrentValue += delta * curveFactor * acceleration * boost * deltaTime;
            }
            else if (enableEaseFunction)
            {
                float t = Mathf.Clamp01(Mathf.Abs(delta));
                float ease = easeFunc(t);
                CurrentValue += delta * ease * acceleration * boost * deltaTime;
            }
            else
            {
                CurrentValue = Mathf.MoveTowards(CurrentValue, rawInput, acceleration * boost * deltaTime);
            }
        }
        else
        {
            CurrentValue = Mathf.MoveTowards(CurrentValue, 0f, deceleration * deltaTime);
            if (Mathf.Abs(CurrentValue) < deadThreshold)
                CurrentValue = 0f;
        }

        CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);
    }

    // 默认缓动函数（EaseOutQuad）
    private float EaseOutQuad(float t)
    {
        return -1f * t * (t - 2f);
    }
}
