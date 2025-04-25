using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public abstract class BTNode
    {
        public enum Status
        {
            Failure = 0,
            Success,
            Running
        }
        public Status currentStatus { get; protected set; }

        public abstract Status Execute();

    }
}
