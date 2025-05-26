using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayNPCSpawnPoint : GamePlaySpawnPoint<GamePlayNPC>
{
    public override GamePlayNPC Spawn<GamePlayNPC>(uint id)
    {
        return null;
    }

#if UNITY_EDITOR

    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("NPC³öÉúµã: ", GamePlayId);
        color = GamePlayId <= 0 ? Color.red : Color.cyan;
        return true;
    }
#endif
}
