using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameScene.FlowNode;
using GameScene.FlowNode.Base;
using System.Collections;
using System.Collections.Generic;
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

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat(GamePlayId, isGen ? " G " : " ", TableId);
        color = GamePlayId <= 0 ? Color.red : Color.white;
        return true;
    }
#endif
}
