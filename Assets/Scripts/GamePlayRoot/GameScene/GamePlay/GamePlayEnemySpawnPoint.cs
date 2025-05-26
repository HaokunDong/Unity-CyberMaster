using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Managers;
using UnityEngine;

public class GamePlayEnemySpawnPoint : GamePlaySpawnPoint<GamePlayEnemy>
{
    public override async UniTask<GamePlayEnemy> Spawn<GamePlayEnemy>()
    {
        var data = EnemyTable.GetTableData(spawnEntityTableId);
        if(data != null)
        {
            var obj = await Managers.ResourceManager.LoadAssetAsync<GameObject>(data.Prefab, ResType.Prefab);
            GamePlayEnemy e = obj.GetComponent<GamePlayEnemy>();
            var enemy = e as GamePlayAIEntity;
            await enemy.InitAI(data.Graph);
            return e;
        }
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
