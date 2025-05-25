namespace GameScene.FlowNode.Base
{
    //用于在蓝图和代码之间传递消息，不用FlowCanvas自带的CustomEvent
    public interface IFlowMessage : IMessage
    {
        
    }
    
    public interface IFlowAsyncMessage : IAsyncMessage
    {
        
    }
}