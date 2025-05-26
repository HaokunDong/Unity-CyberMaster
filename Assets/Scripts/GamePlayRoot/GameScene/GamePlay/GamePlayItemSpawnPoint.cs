using Cysharp.Text;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayItemSpawnPoint : GamePlaySpawnPoint<GamePlayItem>
{
    public override async UniTask<GamePlayItem> Spawn<GamePlayItem>()
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

