using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourTrees
{
    public class Selector : Composer
    {
        private BTNode[] children;
        private int currentChildIndex;

        public Selector(params BTNode[] _children)
        {
            children = _children;
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
