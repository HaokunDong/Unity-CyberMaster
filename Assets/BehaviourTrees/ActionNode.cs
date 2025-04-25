using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public class ActionNode : BTNode
    {
        private Action action;

        public ActionNode(Action action)
        {
            this.action = action;
        }

        public override Status Execute()
        {
            action();
            return Status.Success;
        }
    }
}
