using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameScene.FlowNode;
using GameScene.FlowNode.Base;
using UnityEngine;


public interface IInteractable
{
    Transform Transform { get; }
    bool canInteract { get; }
    void OnInteract();
}
public class GamePlayItem : GamePlayEntity, IInteractable
{
    public Transform Transform => gameObject.transform;
    public bool canInteract => true;

    private GamePlayFlowController flowCtl = null;
    private GamePlayFlowController FlowCtl
    {
        get
        {
            if (flowCtl == null)
            {
                flowCtl = GetComponent<GamePlayFlowController>();
            }
            return flowCtl;
        }
    }
    public override void Init()
    {
        base.Init();
        FlowCtl?.Init();
    }
    public void OnInteract()
    {
        OnInteract2BP().Forget();
    }

    private async UniTask OnInteract2BP()
    {
        if(FlowCtl != null)
        {
            FlowCtl.SendFlowMessage(new OnInteractEventMsg());
            ManagerCenter.Ins.PlayerInputMgr.CanGamePlayInput = false;
            await FlowCtl.SendFlowMessageAsync(new OnInteractAsyncEventMsg());
            ManagerCenter.Ins.PlayerInputMgr.CanGamePlayInput = true;
        }
    }

    public void SendGamePlayMsg<M>(M msg) where M : IFlowMessage
    {
        flowCtl?.SendFlowMessage(msg);
    }

    public async UniTask SendGamePlayAsyncMsg<M>(M msg) where M : IFlowAsyncMessage
    {
        if (flowCtl != null)
        {
            await flowCtl.SendFlowMessageAsync(msg);
        }
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat(GamePlayId, isGen ? " G " : " ", TableId);
        color = GamePlayId <= 0 ? Color.red : Color.white;
        return true;
    }
#endif
}
