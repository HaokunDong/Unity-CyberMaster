using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;

namespace GameScene.FlowNode.Base
{
    public struct OnUnInitEventMsg : IFlowMessage
    {
        
    }
    
    public struct OnUnInitEventAsyncMsg : IFlowAsyncMessage
    {
        
    }
    
    [Name("OnUnInit")]
    [Category("事件Event/Common")]
    public class OnUnInitEvent : BaseFlowEvent<OnUnInitEventMsg>
    {
        
    }
    
    [Name("Handler_OnExit")]
    [Category("事件Event/Handler")]
    public class Handler_OnExit : BaseFlowEventAsync<OnUnInitEventAsyncMsg>
    {
        
    }
    
    [Name("Handler_OnExit_End")]
    [Category("事件Event/Handler")]
    public class Handler_OnExit_End : BaseFlowEventAsyncEnd<OnUnInitEventAsyncMsg>
    {
        
    }
}