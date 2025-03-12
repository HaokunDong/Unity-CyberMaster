using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourTrees
{
    public class Sequencer : Composer
    {
        protected override Status OnEvaluate(Transform agent, Blackboard blackboard)
        {
            bool isRunning = false;
            bool success = children.All((child) =>
            {
                Status status = child.Evaluate(agent, blackboard);
                switch (status)
                {
                    case Status.Failure:
                        return false;
                    case Status.Running:
                        isRunning = true;
                        break;
                }
                return child.Evaluate(agent, blackboard) == Status.Success;
            });

            return isRunning ? Status.Running : success ? Status.Success : Status.Failure;
        }
    }
}
