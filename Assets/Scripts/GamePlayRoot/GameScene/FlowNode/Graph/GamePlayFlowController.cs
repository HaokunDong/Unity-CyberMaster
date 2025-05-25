using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using FlowCanvas.Nodes;
using GameBase.Log;
using GameScene.FlowNode.Base;
using NodeCanvas.Framework;

namespace GameScene.FlowNode
{
    public class GamePlayFlowController : GraphOwner<GamePlayFlowGraph>
    {
        public enum State
        {
            UnInitialized = 0,
            InitializedHandleEvent, //处理AfterInit事件中
            Initialized,
        }

        private State m_state;
        
        public bool IsInitialized => m_state == State.Initialized || m_state == State.InitializedHandleEvent;
        //是否处于Init事件内
        public bool IsDuringInitEvent => m_state == State.InitializedHandleEvent;

        #region 记录是否有异步节点在执行

        private int m_lockCount = 0;
        public bool IsRecordAsyncNodeRun => m_lockCount > 0;
        public int RecordAsyncNodeRun() => ++m_lockCount;
        
        public int UnRecordAsyncNodeRun()
        {
            if(m_lockCount > 0) m_lockCount--;
            return m_lockCount;
        }

        #endregion
        
        
        public void Init()
        {
            // LogUtils.Error($"GamePlayFlowController Init {gameObject.GetPath()}");
            if (!IsInitialized)
            {
                m_messageCenter = new MessageCenter();
                //LoadSaveDataToBlackboard();
                StartBehaviour();
                m_state = State.InitializedHandleEvent;
                SendFlowMessage(new OnInitEventMsg());
                if (graph && graph.allNodes != null)
                {
                    for (var i = 0; i < graph.allNodes.Count; i++)
                    {
                        var node = graph.allNodes[i];
                        
                        if (node is IBaseFlowNode node2)
                        {
                            node2.OnAfterInit();
                        }
                        else if (node is OnVariableChange node3)
                        {
                            node3.OnAfterInit();
                        }
                    }
                }

                m_state = State.Initialized;
            }
            else
            {
                LogUtils.Error("GamePlayFlowController 重复初始化");
            }
        }
        public void UnInit()
        {
            if (IsInitialized)
            {
                SendFlowMessage(new OnUnInitEventMsg());
                m_messageCenter.Dispose();
                m_messageCenter = null;
                StopBehaviour();
                UnLoadBlackboardBridge();
                m_state = State.UnInitialized;
            }
            else
            {
                LogUtils.Error("GamePlayFlowController UnInit异常");
            }
        }

        private MessageCenter m_messageCenter;

        public void SendFlowMessage<T>(in T msg) where T : IFlowMessage
        {
            m_messageCenter?.SendMessage(in msg);
        }
        
        public UniTask SendFlowMessageAsync<T>(in T msg) where T : IFlowAsyncMessage
        {
            return m_messageCenter?.SendAsync(msg) ?? UniTask.CompletedTask;
        }
        
        public void AddFlowListener<T>(MessageDelegate<T> callback) where T : IFlowMessage
        {
            m_messageCenter?.AddListener(callback);
        }

        public void RemoveFlowListener<T>(MessageDelegate<T> callback) where T : IFlowMessage
        {
            m_messageCenter?.RemoveListener(callback);
        }
        
        public void AddFlowListenerAsync<T>(MessageDelegateAsync<T> callback) where T : IFlowAsyncMessage
        {
            m_messageCenter?.AddListenerAsync(callback);
        }
        
        public void RemoveFlowListenerAsync<T>(MessageDelegateAsync<T> callback) where T : IFlowAsyncMessage
        {
            m_messageCenter?.RemoveListenerAsync(callback);
        }

        #region 黑板存档相关

        //private void LoadSaveDataToBlackboard()
        //{
        //    if (graph)
        //    {
        //        var saver = GetOwnerSaver();
        //        foreach (var pair in graph.blackboard.variables)
        //        {
        //            if (pair.Value.IsSaveData)
        //            {
        //                var value = saver.GetSaveData(pair.Key);
        //                if (value != null)
        //                {
        //                    graph.blackboard.SetVariableValue(pair.Key, value);
        //                }

        //                pair.Value.onValueChanged += value =>
        //                {
        //                    saver.SetSaveData(pair.Key, value);
        //                };
        //            }
        //        }
        //    }
        //}
        
        #endregion
        
        #region 蓝图桥接机制

        private Blackboard m_blackboardBridge;

        public void SetBlackboardOverwrite(Blackboard bb)
        {
            // LogUtils.Error($"SetBlackboardBridge {gameObject.GetPath()}");
            UnLoadBlackboardBridge();
            m_blackboardBridge = bb;
            foreach ( var pair in ((IBlackboard)bb).variables )
            {
                var overwriteValue = pair.Value.value;
                if(overwriteValue == null) continue;
                if (graph.blackboard.variables.TryGetValue(pair.Key, out var bbVar))
                {
                    foreach (var parameter in exposedParameters)
                    {
                        if (parameter.targetVariableID == bbVar.ID)
                        {
                            parameter.valueBoxed = overwriteValue;
                            break;
                        }
                    }
                }
                else if ( blackboard != null && blackboard.variables.ContainsKey(pair.Key) ) {
                    blackboard.SetVariableValue(pair.Key, overwriteValue);
                } else
                {
                    LogUtils.Error($"Blackboard Overwrite出错，蓝图黑板缺少SpawnNode上黑板对应参数 key={pair.Key} path={gameObject.GetPath()}", LogChannel.GamePlay,context:gameObject);
                    blackboard.variables[pair.Key] = pair.Value;
                }
            }
        }

        private void UnLoadBlackboardBridge()
        {
            if (m_blackboardBridge != null)
            {
                //TODO:放回对象池时需要重置黑板
                //blackboard.parent
            }
            m_blackboardBridge = null;
        }


        #endregion
#if UNITY_EDITOR

        protected new void Reset()
        {
            //不需要再使用blackboard脚本
            enableAction = GraphOwner.EnableAction.DoNothing;
            disableAction = GraphOwner.DisableAction.DoNothing;
        }
        
#endif
        
    }
}