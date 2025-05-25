using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;

namespace GameScene.FlowNode.Base
{
    public struct OnInitEventMsg : IFlowMessage
    {
        
    }
    
    public struct OnInitEventAsyncMsg : IFlowAsyncMessage
    {
        
    }
    
    [Name("OnInit")]
    [Category("事件Event/Common")]
    public class OnInitEvent : BaseFlowEvent<OnInitEventMsg>
    {
        
    }
    
    
    [Name("Handler_OnEnter")]
    [Category("事件Event/Handler")]
    public class Handler_OnEnter : BaseFlowEventAsync<OnInitEventAsyncMsg>
    {
        
    }
    
    [Name("Handler_OnEnter_End")]
    [Category("事件Event/Handler")]
    public class Handler_OnEnter_End : BaseFlowEventAsyncEnd<OnInitEventAsyncMsg>
    {
        
    }
}