using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

public enum XY
{
    X,
    Y
}

[Name("检查玩家在某个轴的速度分量")]
public class GamePlayPlayerHasVelocityConditionTask : ConditionTask<GamePlayPlayer>
{
    public XY checkAxis = XY.X;
    public BBParameter<float> velocityCheckValue = new BBParameter<float>(0);
    public bool useABS = true;
    public bool useRigidBody = false;
    public CompareMethod checkType = CompareMethod.EqualTo;
    [SliderField(0, 0.1f)]
    public float differenceThreshold = 0.05f;

    protected override string info => $"玩家在{checkAxis}轴的速度{(useABS ? "的绝对值" : "")}是否 {checkType} {velocityCheckValue.value}";

    protected override bool OnCheck()
    {
        var vec = useRigidBody ? agent.rb.velocity : agent.Velocity;
        if (checkAxis == XY.X)
        {
            if(useABS)
            {
                return OperationTools.Compare(Mathf.Abs(vec.x), velocityCheckValue.value, checkType, differenceThreshold);
            }
            else
            {
                return OperationTools.Compare(vec.x, velocityCheckValue.value, checkType, differenceThreshold);
            }
        }
        else
        {
            if (useABS)
            { 
                return OperationTools.Compare(Mathf.Abs(vec.y), velocityCheckValue.value, checkType, differenceThreshold);
            }
            else
            {
                return OperationTools.Compare(vec.y, velocityCheckValue.value, checkType, differenceThreshold);
            }
        }
    }
}
