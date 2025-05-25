using Cysharp.Threading.Tasks;
using FlowCanvas;
using FlowCanvas.Nodes;
using GameBase.Log;
using GameScene.FlowNode.Base;
using ParadoxNotion.Design;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameScene.FlowNode.Function
{
    //改版AND，原版计时逻辑容易引发bug
    [Name("AND")]
    [Category("Flow Controllers/Flow Merge")]
    [Description("Await all flow inputs to be called within the max allowed await time before Out is called.\n- If MaxAwaitTime is 0, then all inputs must be called at the same time.\n- If MaxAwaitTime is -1, then the await time is infinity.")]
    public class ANDMergeGamePlay : BaseFlowControlNode
    {
        [InfoBox("WaitGraphEndReset 当蓝图没有正在运行的Action节点时重置，填时间容易出BUG")]
        public bool waitGraphEndReset = true;

        [SerializeField, ExposeField]
        [ParadoxNotion.Design.MinValue(2), DelayedField]
        [GatherPortsCallback]
        private int _portCount = 2;

        [DisableIf("waitGraphEndReset")]
        public float maxAwaitTime = 0;

        private FlowOutput fOut;
        private float[] calls;
        //Modified: 在Time.frameCount为0时进行Check操作可能会不生效，因此给lastFrameCall赋一个无效的初始值
        private int lastFrameCall = -1;

        public override void OnGraphStarted() { Reset(); }

        protected override void RegisterPorts() {
            calls = new float[_portCount];
            fOut = AddFlowOutput("Out");
            for ( var _i = 0; _i < _portCount; _i++ ) {
                var i = _i;
                AddFlowInput(i.ToString(), (f) => { Check(i, f); });
            }
        }

        void Reset() {
            for ( var i = 0; i < calls.Length; i++ ) {
                calls[i] = float.NegativeInfinity;
            }

            m_waitGraphUnLock = false;
        }

        void Check(int index, Flow f) {
            var t = maxAwaitTime < 0 ? 0 : Time.time;
            calls[index] = t;
            for ( var i = 0; i < calls.Length; i++ ) {
                if (waitGraphEndReset ? calls[i] == float.NegativeInfinity : t - calls[i] > maxAwaitTime)
                {
                    if (waitGraphEndReset && !m_waitGraphUnLock)
                    {
                        //起个task开始等待蓝图运行结束
                        WaitGraphUnLock().Forget();
                    }
                    return;
                }
            }

            if ( Time.frameCount != lastFrameCall ) {
                lastFrameCall = Time.frameCount;
                Reset();
                fOut.Call(f);
            }
        }

        private bool m_waitGraphUnLock = false;

        private async UniTaskVoid WaitGraphUnLock()
        {
            if(m_waitGraphUnLock) return;
            m_waitGraphUnLock = true;
            await UniTask.DelayFrame(1);
            await UniTask.WaitUntil(() =>
            {
                var controller = GetController();
                return !controller.IsRecordAsyncNodeRun || !m_waitGraphUnLock;
            });
            //等到蓝图运行结束时，Reset
            if(m_waitGraphUnLock) Reset();
        }
    }
}