using NodeCanvas.Framework;

public class ConditionalAction : ActionTask
{
    public BBParameter<ConditionTask> condition;
    public BBParameter<ActionTask> task;
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
