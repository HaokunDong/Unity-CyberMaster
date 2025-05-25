using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayNPC : GamePlayAIEntity
{
#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("npc: ", GamePlayId, isGen ? " (¶¯Ì¬Éú³É)" : "");
        color = GamePlayId <= 0 ? Color.red : Color.cyan;
        return true;
    }
#endif
}
