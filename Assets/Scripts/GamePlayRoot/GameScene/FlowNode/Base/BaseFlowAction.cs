using System;
using FlowCanvas;
using GameBase.Log;
using ParadoxNotion.Design;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameScene.FlowNode.Base
{
    [ContextDefinedInputs(typeof(Flow))]
    [ContextDefinedOutputs(typeof(Flow))]
    [Category("行为Action")]
    public abstract class BaseFlowAction : BaseFlowNode
    {
        private FlowInput m_flowInput;
        private FlowOutput m_flowOutput;
        
#if UNITY_EDITOR
        [ShowInInspector, HideInEditorMode, LabelText("运行次数"),ReadOnly]
        private uint m_runTime = 0;
#endif
        
        protected override void RegisterPorts()
        {
            m_flowInput = AddFlowInput("In", flow =>
            {
#if UNITY_EDITOR
                m_runTime++;
#endif
                InvokeFunction(flow);
#if UNITY_EDITOR
                if (!graph.isRunning && m_flowOutput.connections > 0)
                {
                    LogError("蓝图被销毁，无法执行下个节点");
                }
#endif
                m_flowOutput.Call(flow);
            });
            m_flowOutput = AddFlowOutput("Out");
        }
        
        protected abstract void InvokeFunction(in Flow flow);
#if UNITY_EDITOR
        protected override void OnNodeInspectorGUI()
        {
            base.OnNodeInspectorGUI();
            if (GUILayout.Button("调试运行"))
            {
                InvokeFunction(new Flow());
            }
        }
#endif
    }
}