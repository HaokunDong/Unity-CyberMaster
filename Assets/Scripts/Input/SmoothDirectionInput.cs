using UnityEngine;

public class SmoothDirectionInput
{
    public float CurrentValue { get; private set; }

    public float MaxValue = 1f;
    public float MinValue = -1f;

    private float acceleration;
    private float deceleration;
    private float rawInput;
    private bool isAnalog = false;  // 是否是模拟输入（非数字输入），如手柄摇杆等

    private float minStartValue;   // 起步时最小值
    private float deadThreshold;   // 死区值
    private float turnAcceleration;  // 方向相反时的转向加速度

    /// <summary>
    /// 
    /// </summary>
    /// <param name="acceleration">平滑插值加速度</param>
    /// <param name="deceleration">平滑插值减速度</param>
    /// <param name="minStartValue">开始加速平滑插值时最小值</param>
    /// <param name="deadThreshold">开始减速平滑插值时小于一定阈值后归0</param>
    /// <param name="turnAcceleration">方向相反时的转向加速度</param>
    public SmoothDirectionInput(float acceleration, float deceleration, float minStartValue, float deadThreshold, float turnAcceleration = -1f)
    {
        this.acceleration = acceleration;
        this.deceleration = deceleration;
        this.minStartValue = minStartValue;
        this.deadThreshold = deadThreshold;
        this.turnAcceleration = turnAcceleration <= 0f ? deceleration : turnAcceleration; // 默认转向加速度是停止加速度
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
        // 如果是模拟输入（如手柄摇杆），则直接使用原始输入值
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
            // 如果当前速度是 0，直接从 minStartValue 起步
            if (Mathf.Approximately(CurrentValue, 0f))
            {
                CurrentValue = minStartValue * Mathf.Sign(rawInput);
            }

            // 如果 rawInput 和当前方向相反，表示转向，需要快速转向
            if (Mathf.Sign(rawInput) != Mathf.Sign(CurrentValue))
            {
                CurrentValue = Mathf.MoveTowards(CurrentValue, rawInput, turnAcceleration * deltaTime);
            }
            else
            {
                CurrentValue = Mathf.MoveTowards(CurrentValue, rawInput, acceleration * deltaTime);
            }
        }
        else
        {
            // 减速
            CurrentValue = Mathf.MoveTowards(CurrentValue, 0f, deceleration * deltaTime);

            if (Mathf.Abs(CurrentValue) < deadThreshold)
            {
                CurrentValue = 0f;
            }
        }

        CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);
    }
}
