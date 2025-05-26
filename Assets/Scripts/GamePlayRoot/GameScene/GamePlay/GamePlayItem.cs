using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayItem : GamePlayEntity
{
#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("item: ", GamePlayId, isGen ? " (¶¯Ì¬Éú³É)" : "");
        color = GamePlayId <= 0 ? Color.red : Color.white;
        return true;
    }
#endif
}
