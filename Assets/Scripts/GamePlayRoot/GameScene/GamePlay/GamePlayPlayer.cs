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

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat(GamePlayId);
        color = Color.red;
        return true;
    }
#endif
}
