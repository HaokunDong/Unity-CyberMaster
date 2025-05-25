using Cysharp.Threading.Tasks;
using FlowCanvas;
using GameBase.Log;
using GameScene.FlowNode.Base;
using ParadoxNotion.Design;

namespace GameScene.FlowNode.Function
{
    public abstract class LogNodeBase<T> : BaseFlowAction
    {
        public enum LogTypeForGraph
        {
            Trace = 0,
            Debug = 1,
            Warning = 2,
            Error = 3,
        }
        
        private ValueInput<T> m_value;
        public LogTypeForGraph logType;
        
        protected override void RegisterPorts()
        {
            base.RegisterPorts();
            m_value = AddValueInput<T>("Value");
            
        }
        protected override void InvokeFunction(in Flow flow)
        {
            switch (logType)
            {
                case LogTypeForGraph.Debug:
                    LogUtils.Debug(m_value.value);
                    break;
                case LogTypeForGraph.Trace:
                    LogUtils.Trace(m_value.value, LogChannel.GamePlay);
                    break;
                case LogTypeForGraph.Warning:
                    LogUtils.Warning(m_value.value);
                    break;
                case LogTypeForGraph.Error:
                    LogUtils.Error(m_value.value);
                    break;
            }
        }
    }
    
    [Name("LogText")]
    [Category("常用")]
    public class LogText : LogNodeBase<string>
    {
       
    }
    
    [Name("LogValue")]
    [Category("常用")]
    public class LogValue : LogNodeBase<object>
    {
       
    }
}