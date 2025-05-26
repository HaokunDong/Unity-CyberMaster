using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayItemSpawnPoint : GamePlaySpawnPoint<GamePlayItem>
{
    public override GamePlayItem Spawn<GamePlayItem>(uint id)
    {
        return null;
    }

#if UNITY_EDITOR

    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("Item³öÉúµã: ", GamePlayId);
        color = GamePlayId <= 0 ? Color.red : Color.white;
        return true;
    }
#endif
}

