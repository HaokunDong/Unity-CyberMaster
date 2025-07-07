using Cysharp.Threading.Tasks;
using FlowCanvas;
using ParadoxNotion.Design;
using Tools;
using UnityEditor;
using UnityEngine;

namespace GameScene.FlowNode.Base
{
    //这类事件必须要配一个End节点
    [Category("Events")]
    [Color("ff5c5c")]
    [ContextDefinedOutputs(typeof(Flow))]
    [ExecutionPriority(1)]
    public class BaseFlowEventAsync<T> : BaseFlowNode where T : IFlowAsyncMessage
    {
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
        
        private FlowOutput m_eventOut;
        private T msg;


        public override void OnGraphStarted()
        {
            var control = graphBlackboard.propertiesBindTarget as GamePlayFlowController;
            control.AddFlowListenerAsync<T>(OnMessageReceive);
        }

        public override void OnGraphStoped()
        {
            var control = graphBlackboard.propertiesBindTarget as GamePlayFlowController;
            control.RemoveFlowListenerAsync<T>(OnMessageReceive);
        }

        protected virtual UniTask OnMessageReceive(in T msg)
        {
            AutoResetUniTaskCompletionSource taskSource = AutoResetUniTaskCompletionSource.Create();
            this.msg = msg;
            var flow = CreateFlow(in msg);
            flow.SetReturnData(value =>
            {
                //单纯TrySetResult存在被调用多次的风险
                //taskSource.TrySetResult();
                UniTaskUtils.TrySetResultAndSafeSetNull(ref taskSource);
            }, null);
            m_eventOut.Call(flow);
            return taskSource?.Task ?? UniTask.CompletedTask;
        }

        protected virtual Flow CreateFlow(in T msg)
        {
            return new Flow();
        }

        protected override void RegisterPorts()
        {
            m_eventOut = AddFlowOutput("OnEvent");
            AddValueOutput<T>("Message", () => { return msg; });
        }
        
#if UNITY_EDITOR
        protected override void OnNodeInspectorGUI()
        {
            base.OnNodeInspectorGUI();
            EditorGUI.BeginDisabledGroup(!graph.isRunning);
            if (GUILayout.Button("调试触发"))
            {
                var flow = CreateFlow(default);
                flow.SetReturnData(value =>
                {
                }, null);
                m_eventOut.Call(new Flow());
            }
            EditorGUI.EndDisabledGroup();
        }
#endif
    }

    [Color("bf7fff")]
    public class BaseFlowEventAsyncEnd<T> : BaseFlowNode where T : IFlowAsyncMessage
    {
        public override string name {
            get
            {
                var str = base.name;
                int firstSpace;
                if ((firstSpace = str.IndexOf(' ')) > -1)
                {
                    str = str.Substring(0, firstSpace);
                }
                return str;
            }
        }
        
        private FlowInput m_inPort;
        
        protected override void RegisterPorts()
        {
            m_inPort = AddFlowInput("OnEventEnd", flow =>
            {
                flow.Return(null, this);
            });
        }
    }
}