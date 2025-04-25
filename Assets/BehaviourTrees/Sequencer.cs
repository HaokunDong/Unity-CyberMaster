using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourTrees
{
    public class Sequencer : Composer
    {
        /*protected override Status OnEvaluate(Transform agent, Blackboard blackboard)
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
        }*/
        private BTNode[] children;
        private int currentChildIndex;

        public Sequencer(params BTNode[] _children)
        {
            this.children = _children;
            currentChildIndex = 0;
        }

        public override Status Execute()
        {
            if(currentChildIndex >= children.Length)
            {
                currentChildIndex = 0;
                return Status.Success;
            }

            var childStatus = children[currentChildIndex].Execute();

            switch (childStatus)
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    currentChildIndex = 0;
                    return Status.Failure;
                case Status.Success:
                    currentChildIndex++;
                    if (currentChildIndex < children.Length)
                        return Status.Running;
                    else
                    {
                        currentChildIndex = 0;
                        return Status.Success;
                    }
                default:
                    return Status.Failure;
            }
        }
    }
}
