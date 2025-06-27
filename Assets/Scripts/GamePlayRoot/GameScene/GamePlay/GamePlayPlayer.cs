using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayPlayer : GamePlayEntity
{
    public float maxInteractDistance = 5f;

    public Vector2 GetFacingDirection()
    {
        return transform.right;
    }

    private void Update()
    {
        if (ManagerCenter.Ins.PlayerInputMgr.CanGamePlayInput)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                GamePlayRoot.Current?.InteractTarget?.OnInteract();
            }
        } 
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat(GamePlayId);
        color = Color.red;
        return true;
    }
#endif
}
