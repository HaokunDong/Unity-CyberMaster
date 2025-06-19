using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayItemSpawnPoint : GamePlaySpawnPoint<GamePlayItem>
{
    public override async UniTask<GamePlayItem> Spawn<GamePlayItem>()
    {
        var data = ItemTable.GetTableData(spawnEntityTableId);
        if (data != null)
        {
            var obj = await ResourceManager.LoadAssetAsync<GameObject>(data.Prefab, ResType.Prefab);
            GamePlayItem item = obj.GetComponent<GamePlayItem>();
            return item;
        }
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

