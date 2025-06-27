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

    public struct OnTriggerEventMsg : IFlowMessage
    {
        public uint TriggerGamePlayId;
        public TriggerEventType Type;
    }

    public struct OnInteractEventMsg : IFlowMessage
    {

    }

    public struct OnInteractAsyncEventMsg : IFlowAsyncMessage
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

    [Name("Trigger触发")]
    [Category("事件Event/Common")]
    public class OnTriggerEvent : BaseFlowEvent<OnTriggerEventMsg>
    {

    }

    [Name("瞬间交互")]
    [Category("事件Event/Common")]
    public class OnInteractEvent : BaseFlowEvent<OnInteractEventMsg>
    {

    }

    [Name("非瞬间交互 开始")]
    [Category("事件Event/Common")]
    public class Interact_OnEnter : BaseFlowEventAsync<OnInteractAsyncEventMsg>
    {

    }

    [Name("非瞬间交互 结束")]
    [Category("事件Event/Common")]
    public class Interact_OnEnter_End : BaseFlowEventAsyncEnd<OnInteractAsyncEventMsg>
    {

    }
}