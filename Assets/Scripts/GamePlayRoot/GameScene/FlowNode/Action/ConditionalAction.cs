using NodeCanvas.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalAction : ActionTask
{
    public BBParameter<ConditionTask> condition;
    public BBParameter<ActionTask> task;

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
        }
    }
}
