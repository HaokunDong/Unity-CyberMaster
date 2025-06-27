using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Managers;
using UnityEngine;

public class GamePlayNPCSpawnPoint : GamePlaySpawnPoint<GamePlayNPC>
{
    public bool spawnedEnemyPauseWhenInvisible = true;

    public override async UniTask<GamePlayNPC> Spawn<GamePlayNPC>()
    {
        var data = NPCTable.GetTableData(spawnEntityTableId);
        if (data != null)
        {
            var obj = await ResourceManager.LoadAssetAsync<GameObject>(data.Prefab, ResType.Prefab);
            GamePlayNPC npc = obj.GetComponent<GamePlayNPC>();
            var aie = npc as GamePlayAIEntity;
            if (aie != null)
            {
                aie.graphPath = data.AIPath;
                aie.pauseWhenInvisible = spawnedEnemyPauseWhenInvisible;
            }
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
