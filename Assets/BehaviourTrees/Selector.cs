using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourTrees
{
    public class Selector : Composer
    {
        /*protected override Status OnEvaluate(Transform agent, Blackboard blackboard)
        {
            bool isRunning = false;
            bool failed = children.All((child) =>
            {
                Status status = child.Evaluate(agent, blackboard);
                if(status == Status.Running) isRunning = true;
                return status == Status.Failure;
            });

            return isRunning ? Status.Running : failed ? Status.Failure : Status.Success;
        }*/

        private BTNode[] children;
        private int currentChildIndex;

        public Selector(params BTNode[] _children)
        {
            this.children = _children;
            currentChildIndex = 0;
        }

        public override Status Execute()
        {
            if(currentChildIndex >= children.Length)
            {
                currentChildIndex = 0;
                return Status.Failure;
            }

            var childStatus = children[currentChildIndex].Execute();

            switch (childStatus)
            {
                case Status.Running:
                    return Status.Running;
                case Status.Success:
                    currentChildIndex = 0;
                    return Status.Success;
                case Status.Failure:
                    currentChildIndex++;
                    if (currentChildIndex < children.Length)
                        return Status.Running;
                    else
                    {
                        currentChildIndex = 0;
                        return Status.Failure;
                    }
                default:
                    return Status.Failure;
            }
        }
    }
}
