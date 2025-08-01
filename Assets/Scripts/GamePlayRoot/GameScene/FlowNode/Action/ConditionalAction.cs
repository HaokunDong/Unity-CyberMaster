using NodeCanvas.Framework;
using ParadoxNotion.Design;

public class ConditionalAction : ActionTask
{
    [Header("判断条件")]
    public BBParameter<ConditionTask> condition;
    [Header("判断条件为真时")]
    public BBParameter<ActionTask> task;
    [Header("判断条件为假时")]
    public BBParameter<ActionTask> elseTask;

    protected override void OnExecute()
    {
        base.OnExecute();
        if(condition.value != null)
        {
            condition.value.Enable(agent, blackboard);
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (condition.value != null)
        {
            if (condition.value.Check(agent, blackboard))
            {
                task.value.Execute(agent, blackboard);
            }
            else
            {
                if(!elseTask.isNoneOrNull)
                {
                    elseTask.value.Execute(agent, blackboard);
                }
            }
        }
    }
}
