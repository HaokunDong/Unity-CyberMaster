using FlowCanvas;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GameScene.FlowNode.Base
{
    //基本的事件监听节点
    [Category("事件Event")]
    [Color("ff5c5c")]
    [ContextDefinedOutputs(typeof(Flow))]
    [ExecutionPriority(1)]
    public abstract class BaseFlowEvent : BaseFlowNode
    {
        protected FlowOutput m_eventOut;
        
        public override string name {
            get
            {
                var str = base.name;
                int firstSpace;
                if ((firstSpace = str.IndexOf(' ')) > -1)
                {
                    str = str.Substring(0, firstSpace);
                }
                return $"➥ {str}";
            }
        }
        
        protected override void RegisterPorts()
        {
            m_eventOut = AddFlowOutput("OnEvent");
        }
        
#if UNITY_EDITOR
        protected override void OnNodeInspectorGUI()
        {
            base.OnNodeInspectorGUI();
            EditorGUI.BeginDisabledGroup(!graph.isRunning);
            if (GUILayout.Button("调试触发"))
            {
                m_eventOut.Call(new Flow());
            }
            EditorGUI.EndDisabledGroup();
        }
#endif
    }

    [Category("事件Event")]
    public abstract class BaseFlowEvent<T> : BaseFlowEvent where T : IFlowMessage
    {
        private T msg;

        protected override void RegisterPorts()
        {
            base.RegisterPorts();
            AddValueOutput<T>("Message", () => { return msg; });
        }

        public override void OnCreate(Graph assignedGraph)
        {
            if (assignedGraph.isRunning)
                OnGraphStarted();
        }
        
        public override void OnGraphStarted()
        {
            var control = graphBlackboard.propertiesBindTarget as GamePlayFlowController;
            control.AddFlowListener<T>(OnMessageReceive);
        }

        protected virtual void OnMessageReceive(in T msg)
        {
            this.msg = msg;
            m_eventOut.Call(new Flow());
        }
        
        public override void OnGraphStoped()
        {
            var control = graphBlackboard.propertiesBindTarget as GamePlayFlowController;
            control.RemoveFlowListener<T>(OnMessageReceive);
        }
    }
    
    //全局Message版本
    [Category("事件Event")]
    public abstract class BaseFlowEventMessage<T> : BaseFlowEvent where T : IMessage
    {
        public override void OnCreate(Graph assignedGraph)
        {
            if (assignedGraph.isRunning)
                OnGraphStarted();
        }

        public override void OnGraphStarted()
        {
            Message.AddListener<T>(OnMessageReceive);
        }

        protected virtual void OnMessageReceive(in T msg)
        {
            m_eventOut.Call(new Flow());
        }
        
        public override void OnGraphStoped()
        {
            Message.RemoveListener<T>(OnMessageReceive);
        }
    }
}