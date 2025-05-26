using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayEnemySpawnPoint : GamePlaySpawnPoint<GamePlayEnemy>
{
    public override GamePlayEnemy Spawn<GamePlayEnemy>(uint id)
    {
        return null;
    }

#if UNITY_EDITOR

    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("Enemy³öÉúµã: ", GamePlayId);
        color = GamePlayId <= 0 ? Color.red : Color.yellow;
        return true;
    }
#endif
}
