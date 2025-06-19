using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayNPC : GamePlayAIEntity, IInteractable
{
    public Transform Transform => gameObject.transform;
    public bool canInteract => true;

    public void OnInteract()
    {
        throw new System.NotImplementedException();
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat(GamePlayId, isGen ? " G " : " ", TableId);
        color = GamePlayId <= 0 ? Color.red : Color.cyan;
        return true;
    }
#endif
}
