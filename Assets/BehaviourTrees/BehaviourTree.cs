using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    [RequireComponent(typeof(Blackboard))]
    public class BehaviourTree : MonoBehaviour
    {
        private Node root;
        public Node Root
        {
            get => root;
            protected set => root = value;
        }

        private Blackboard blackboard;
        public Blackboard Blackboard
        {
            get => blackboard;
            set => blackboard = value;
        }

        
        private void Awake()
        {
            blackboard = GetComponent<Blackboard>();
            OnSetup();
        }
        
        void Update()
        {
            root?.Evaluate(gameObject.transform, blackboard);
        }

        protected virtual void OnSetup()
        {

        }
    }
}
