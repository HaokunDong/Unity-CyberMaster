using UnityEngine;

public class SmoothDirectionInput
{
    public float CurrentValue { get; private set; }

    public float MaxValue = 1f;
    public float MinValue = -1f;

    private float acceleration;
    private float deceleration;
    private float rawInput;
    private bool isAnalog = false;

    private float minStartValue;   // 起步时最小值
    private float deadThreshold;   // 死区值

    /// <summary>
    /// 
    /// </summary>
    /// <param name="acceleration">平滑插值加速度</param>
    /// <param name="deceleration">平滑插值减速度</param>
    /// <param name="minStartValue">开始加速平滑插值时最小值</param>
    /// <param name="deadThreshold">开始减速平滑插值时小于一定阈值后归0</param>
    public SmoothDirectionInput(float acceleration, float deceleration, float minStartValue, float deadThreshold)
    {
        this.acceleration = acceleration;
        this.deceleration = deceleration;
        this.minStartValue = minStartValue;
        this.deadThreshold = deadThreshold;
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
            CurrentValue = rawInput;
            if (Mathf.Abs(CurrentValue) < deadThreshold)
            {
                CurrentValue = 0f;
            }
            else if (Mathf.Abs(CurrentValue) < minStartValue)
            {
                CurrentValue = minStartValue;
            }

            CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);
            return;
        }

        // 有输入时加速
        if (Mathf.Abs(rawInput) > 0.01f)
        {
            // 起步时给一个初始值
            if (Mathf.Approximately(CurrentValue, 0f))
            {
                CurrentValue = minStartValue * Mathf.Sign(rawInput);
            }

            CurrentValue = Mathf.MoveTowards(CurrentValue, rawInput, acceleration * deltaTime);
        }
        else
        {
            // 减速
            CurrentValue = Mathf.MoveTowards(CurrentValue, 0f, deceleration * deltaTime);

            // 速度很小了，直接归零
            if (Mathf.Abs(CurrentValue) < deadThreshold)
            {
                CurrentValue = 0f;
            }
        }

        CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);
    }
}
