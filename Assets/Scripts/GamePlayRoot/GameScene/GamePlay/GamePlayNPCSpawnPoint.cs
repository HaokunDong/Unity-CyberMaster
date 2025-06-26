using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayNPCSpawnPoint : GamePlaySpawnPoint<GamePlayNPC>
{
    public override async UniTask<GamePlayNPC> Spawn<GamePlayNPC>()
    {
        var data = NPCTable.GetTableData(spawnEntityTableId);
        if (data != null)
        {
            var obj = await ResourceManager.LoadAssetAsync<GameObject>(data.Prefab, ResType.Prefab);
            GamePlayNPC npc = obj.GetComponent<GamePlayNPC>();
            return npc;
        }
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
