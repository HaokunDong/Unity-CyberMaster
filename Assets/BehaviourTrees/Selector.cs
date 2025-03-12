using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourTrees
{
    public class Selector : Composer
    {
        protected override Status OnEvaluate(Transform agent, Blackboard blackboard)
        {
            bool isRunning = false;
            bool failed = children.All((child) =>
            {
                Status status = child.Evaluate(agent, blackboard);
                if(status == Status.Running) isRunning = true;
                return status == Status.Failure;
            });

            return isRunning ? Status.Running : failed ? Status.Failure : Status.Success;
        }
    }
}
