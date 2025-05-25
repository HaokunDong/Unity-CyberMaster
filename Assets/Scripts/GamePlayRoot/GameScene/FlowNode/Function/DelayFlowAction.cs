using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using FlowCanvas;
using GameBase.Log;
using GameScene.FlowNode.Base;
using ParadoxNotion.Design;
using Tools;

namespace GameScene.FlowNode.Function
{
    [Name("DelayAction")]
    [Category("常用")]
    public class DelayFlowAction : BaseFlowActionAsync
    {
        private ValueInput<float> m_delayTime;

        protected override void RegisterPorts()
        {
            base.RegisterPorts();
            m_delayTime = AddValueInput<float>("DelayTime(s)");
        }

        protected override UniTask InvokeFunction(Flow flow)
        {
            var timeValue = m_delayTime.value;
            if (timeValue <= 0)
            {
                if (timeValue < 0)
                {
                    LogUtils.Error($"DelayFlowAction 传入时间为负 Time:{timeValue} Path:{GetController().gameObject.GetPath()}");
                }
                return UniTask.CompletedTask;
            }
            return UniTask.Delay(timeValue.SecondToMillisecondInt());
        }
    }
    
    [Name("DelayFrameAction")]
    [Category("常用")]
    public class DelayFrameAction : BaseFlowActionAsync
    {
        private ValueInput<int> m_delayFrame;

        protected override void RegisterPorts()
        {
            base.RegisterPorts();
            m_delayFrame = AddValueInput<int>("DelayFrames");
        }

        protected override UniTask InvokeFunction(Flow flow)
        {
            var timeValue = m_delayFrame.value;
            if (timeValue <= 0)
            {
                if (timeValue < 0)
                {
                    LogUtils.Error($"DelayFrameAction 传入参数为负 Time:{timeValue} Path:{GetController().gameObject.GetPath()}");
                }
                return UniTask.CompletedTask;
            }
            return UniTask.DelayFrame(timeValue);
        }
    }
}