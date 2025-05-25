using FlowCanvas.Nodes;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace FlowCanvas.Nodes
{
    public abstract class OnVariableChange : ParameterVariableNode
    {
        [LabelText("Init时触发")]
        public bool callInit = true;
        protected FlowOutput output;
        
        public virtual void OnAfterInit()
        {
            if (callInit)
            {
                output.Call(new Flow());
            }
        }
    }
    
    [Name("OnVariableChange", 10)]
    [Category("Variables/Blackboard")]
    [Description("监听Blackboard数据变化")]
    [Color("ff5c5c")]
    [ContextDefinedOutputs(typeof(Flow))]
    [ContextDefinedInputs(typeof(Wild))]
    public class OnVariableChange<T> : OnVariableChange
    {
        [BlackboardOnly] public BBParameter<T> targetVariable;
        
        public override BBParameter parameter => targetVariable;

        public override void OnAfterInit()
        {
            parameter.varRef.onValueChanged += o =>
            {
                output.Call(new Flow());
            };
            base.OnAfterInit();
        }

        public override string name {
            get { return string.Format("➥ {0}{1}","OnChangeEvent ", targetVariable.ToString()); }
        }
        
        protected override void RegisterPorts() {
            output = AddFlowOutput("Out");
            AddValueOutput<T>("Value", () => { return targetVariable.value; });
        }

#if UNITY_EDITOR
        protected override void OnNodeInspectorGUI()
        {
            base.OnNodeInspectorGUI();
            EditorGUI.BeginDisabledGroup(!graph.isRunning);
            if (GUILayout.Button("调试触发"))
            {
                output.Call(new Flow());
            }
            EditorGUI.EndDisabledGroup();
        }
#endif
    }
}