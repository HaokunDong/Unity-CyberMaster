using System.Threading;
using Cysharp.Threading.Tasks;
using FlowCanvas;
using GameScene.FlowNode.Base;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Sirenix.OdinInspector;
using Tools;
using UnityEngine;

namespace GameScene.FlowNode.Function
{
    [Name("DelayDoTimesAction")]
    [Category("常用")]
    [Description("Delay循环执行多次")]
    public class DelayDoTimesAction : BaseFlowActionAsync
    {
        public BBParameter<float> delayTime = 1f;
        [InfoBox("0表示次数无限，nowTime首次从0开始")] public BBParameter<int> runTimes = 1;

        private CancellationTokenSource m_cancellationTokenSource;
        private int m_nowTime;
        private FlowOutput m_update;
        private FlowOutput m_eachDoFinish;

        protected override void RegisterPorts()
        {
            base.RegisterPorts();
            AddFlowInput("Cancel", delegate(Flow flow)
            {
                m_cancellationTokenSource?.Cancel();
                m_cancellationTokenSource = null;
            });
            AddValueOutput("NowTime", () => m_nowTime);
            m_update = AddFlowOutput("OnUpdate");
            m_eachDoFinish = AddFlowOutput("EachDoFinish");
        }

        protected override async UniTask InvokeFunction(Flow flow)
        {
            if (runTimes.value == 0 && delayTime.value <= 0f)
            {
                LogError("节点参数错误，runTimes为0且delayTime为0");
                return;
            }

            m_cancellationTokenSource ??= new CancellationTokenSource();
            m_nowTime = 0;

            while (runTimes.value == 0 || m_nowTime < runTimes.value)
            {
                float startTime = Time.time;
                await UniTask.WaitUntil(() =>
                {
                    m_update.Call(flow);
                    return Time.time - startTime >= delayTime.value;
                }, cancellationToken: m_cancellationTokenSource.Token);
                m_eachDoFinish.Call(flow);
                m_nowTime++;
            }
        }
    }
}