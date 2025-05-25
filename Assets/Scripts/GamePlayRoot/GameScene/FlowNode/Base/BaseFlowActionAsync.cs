using System;
using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using FlowCanvas;
using GameBase.Log;
using ParadoxNotion.Design;
using Sirenix.OdinInspector;
using Tools;
using UnityEngine;

namespace GameScene.FlowNode.Base
{
    [ContextDefinedInputs(typeof(Flow))]
    [ContextDefinedOutputs(typeof(Flow))]
    public abstract class BaseFlowActionAsync : BaseFlowNode
    {
        [LabelText("执行期间禁止存档（若该节点会被循环执行，要选false）")]
        public bool forbidSaveInRunning = true;
        private FlowInput m_flowInput;
        private FlowOutput m_flowOutput;
        private FlowOutput m_flowOutputFinish;
        
#if UNITY_EDITOR
        [ShowInInspector, HideInEditorMode, LabelText("运行次数"),ReadOnly]
        private uint m_runTime = 0;
#endif
        
        protected override void RegisterPorts()
        {
            m_flowInput = AddFlowInput("In ", flow =>
            {
#if UNITY_EDITOR
                m_runTime++;
#endif
                RunInvokeAsync(flow).Forget();
                m_flowOutput.Call(flow);
            });
            m_flowOutput = AddFlowOutput("OnStart");
            RegisterOutPorts();
        }

        protected virtual void RegisterOutPorts()
        {
            m_flowOutputFinish = AddFlowOutput("OnFinish");
        }

        protected virtual bool IsForbidSaveInRunning => forbidSaveInRunning;
#if UNITY_EDITOR
        [LabelText("该节点位于不可存档状态"), ShowInInspector, HideInEditorMode, ReadOnly]
        private bool isCannotSave = false;
#endif
        private async UniTaskVoid RunInvokeAsync(Flow flow)
        {
#if UNITY_EDITOR
            // if(IsForbidSaveInRunning) LogUtils.Trace($"AddCannotSaveState {GetController().gameObject.GetPath()} {GetType().FullName}", LogChannel.GamePlay);
            isCannotSave = true;
#endif
            
            ////var id = IsForbidSaveInRunning ? SaveUtils.AddCannotSaveState() : -1;
            ////SaveUtils.RecordAddCannotSaveStateSource(id, $"蓝图 Path:{GetController().gameObject.GetPath()} Node:{GetType().FullName}");
            ////if(IsForbidSaveInRunning) LockGamePlayRootLogicUntilFinish();
            try
            {
                await InvokeFunction(flow);
            }
            catch (Exception e)
            {
                Error(e);
            }
            finally
            {
//                if (id > -1)
//                {
//#if UNITY_EDITOR
//                    // LogUtils.Trace($"RemoveCannotSaveState {GetController().gameObject.GetPath()} {GetType().FullName}", LogChannel.GamePlay);
//                    isCannotSave = false;
//#endif
//                    SaveUtils.RemoveCannotSaveState(id);
//                }

#if UNITY_EDITOR
                if (!graph.isRunning && m_flowOutputFinish.connections > 0)
                {
                    LogError("蓝图被销毁，无法执行下个节点");
                }
#endif
                CallOutput(in flow);
                if (m_isLockRootLogic)
                {
                    m_isLockRootLogic = false;
                    var controller = GetController();
                    if (controller)
                    {
                        controller.UnRecordAsyncNodeRun();
                    }
                }
            }
        }
        
        protected virtual void CallOutput(in Flow flow)
        {
            m_flowOutputFinish.Call(flow);
        }

        private bool m_isLockRootLogic = false;
        
        protected abstract UniTask InvokeFunction(Flow flow);

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