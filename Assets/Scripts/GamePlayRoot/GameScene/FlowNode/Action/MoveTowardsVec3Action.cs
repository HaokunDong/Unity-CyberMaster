using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("GamePlay")]
public class MoveTowardsVec3Action : ActionTask<Transform>
{

    [RequiredField]
    public BBParameter<Vector3> target;
    public BBParameter<float> speed = 2;
    public BBParameter<float> stopDistance = 0.1f;
    public bool waitActionFinish;

    protected override void OnExecute()
    {
        Vector3 toPoint = target.value - agent.position;
        if(Vector3.Dot(agent.right, toPoint) <= 0)
        {
            var gpe = agent.GetComponent<GamePlayEntity>();
            gpe?.Flip();
        }
    }

    protected override void OnUpdate()
    {
        if ((agent.position - target.value).magnitude <= stopDistance.value)
        {
            EndAction();
            return;
        }

        agent.position = Vector3.MoveTowards(agent.position, target.value, speed.value * Time.deltaTime);
        if (!waitActionFinish)
        {
            EndAction();
        }
    }
}