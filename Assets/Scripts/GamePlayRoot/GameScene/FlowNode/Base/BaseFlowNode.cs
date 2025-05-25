using System;
using Everlasting.Extend;
using FlowCanvas;
using GameBase.Log;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace GameScene.FlowNode.Base
{
    public interface IBaseFlowNode
    {
        void OnAfterInit();
    }
    
    public abstract class BaseFlowNode : FlowCanvas.FlowNode, IBaseFlowNode
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
                return $"{str}";
            }
        }

        public virtual void OnAfterInit(){}

        protected GamePlayFlowController GetController() => 
            graphAgent as GamePlayFlowController;
        
        protected GamePlayFlowGraph GetGraph() => graph as GamePlayFlowGraph;

#if !UNITY_EDITOR && (LOG_DISABLE || LOG_TRACE_DISABLE)
        [Conditional("FALSE")]
#endif
        public void LogTrace(object log, LogChannel channel = LogChannel.GamePlay, Color? color = null)
        {
            var controller = GetController();
            var goPath = controller ? controller.gameObject.GetPath() : "";
            LogUtils.Trace($"{log} 蓝图:{goPath} Node:{GetType().FullName}", channel, color, controller ? controller.gameObject : null);
        }
        
#if !UNITY_EDITOR && LOG_DISABLE
        [Conditional("FALSE")]
#endif
        public void LogDebug(object log, LogChannel channel = LogChannel.GamePlay, Color? color = null)
        {
            var controller = GetController();
            var goPath = controller ? controller.gameObject.GetPath() : "";
            LogUtils.Debug($"{log} 蓝图:{goPath} Node:{GetType().FullName}", channel, color, controller ? controller.gameObject : null);
        }
        
#if !UNITY_EDITOR && LOG_DISABLE
        [Conditional("FALSE")]
#endif
        public void LogWarning(object log, LogChannel channel = LogChannel.GamePlay, Color? color = null)
        {
            var controller = GetController();
            var goPath = controller ? controller.gameObject.GetPath() : "";
            LogUtils.Warning($"{log} 蓝图:{goPath} Node:{GetType().FullName}", channel, color, controller ? controller.gameObject : null);
        }
        
#if !UNITY_EDITOR && LOG_DISABLE
        [Conditional("FALSE")]
#endif
        public void LogError(object log, LogChannel channel = LogChannel.GamePlay, Color? color = null)
        {
            var controller = GetController();
            var goPath = controller ? controller.gameObject.GetPath() : "";
            LogUtils.Error($"{log} 蓝图:{goPath} Node:{GetType().FullName}", channel, color, controller ? controller.gameObject : null);
            SetStatus(Status.Error);
        }
        
#if UNITY_EDITOR
        //方便搜索
        [NonSerialized]
        public string editorExtendName = "";
        public override string ToString()
        {
            return editorExtendName.IsNullOrEmpty() ? base.ToString() : base.ToString() + editorExtendName;
        }
#endif
    }
    
    [Category("Flow Controllers")]
    [Color("bf7fff")]
    [ContextDefinedInputs(typeof(Flow))]
    [ContextDefinedOutputs(typeof(Flow))]
    abstract public class BaseFlowControlNode : BaseFlowNode { }
}