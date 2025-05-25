using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayEnemy : GamePlayAIEntity
{
#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("enemy: ", GamePlayId, isGen ? " (¶¯Ì¬Éú³É)" : "");
        color = GamePlayId <= 0 ? Color.red : Color.yellow;
        return true;
    }
#endif
}
