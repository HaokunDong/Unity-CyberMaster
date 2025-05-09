using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public class RandomSelector : Composer
    {
        private BTNode[] children;
        private int currentChildIndex;

        public RandomSelector(params BTNode[] _children)
        {
            children = _children;
            currentChildIndex = -1;
        }

        public override Status Execute()
        {
            if(currentChildIndex == -1)
            {
                currentChildIndex = Random.Range(0, children.Length);
                Debug.Log("currentChildIndex: " + currentChildIndex);
            }

            var childStatus = children[currentChildIndex].Execute();

            switch (childStatus)
            {
                case Status.Running:
                    return Status.Running;
                case Status.Success:
                    currentChildIndex = -1;
                    return Status.Success;
                case Status.Failure:
                    /*List<int> remainingIndices = new List<int>();
                    for(int i = 0; i < children.Length; i++)
                    {
                        if(i != currentChildIndex)
                        {
                            remainingIndices.Add(i);
                        }
                    }
                    if(remainingIndices.Count > 0)
                    {
                        currentChildIndex = remainingIndices[Random.Range(0, remainingIndices.Count)];
                        return Status.Running;
                    }*/
                    currentChildIndex = -1;
                    return Status.Failure;
                default:
                    currentChildIndex = -1;
                    return Status.Failure;
            }
        }
    }
}
