using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public enum Status
    {
        Failure = 0,
        Success,
        Running
    }

    public abstract class Node
    {
        protected Node parent;
        protected List<Node> children = new();

        private Status status;

        public Status Status
        {
            get => status;
            protected set => status = value;
        }

        public Status Evaluate(Transform agent, Blackboard blackboard)
        {
            status = OnEvaluate(agent, blackboard);
            return status;
        }
        protected abstract Status OnEvaluate(Transform agent, Blackboard blackboard);
    }
}
