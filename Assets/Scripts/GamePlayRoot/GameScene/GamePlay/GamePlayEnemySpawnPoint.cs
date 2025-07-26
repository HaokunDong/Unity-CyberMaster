using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Managers;
using UnityEngine;

public class GamePlayEnemySpawnPoint : GamePlaySpawnPoint<GamePlayEnemy>
{
    public bool spawnedEnemyPauseWhenInvisible = true;

    public override async UniTask<GamePlayEntity> SpawnEntity()
    {
        var data = EnemyTable.GetTableData(spawnEntityTableId);
        if(data != null)
        {
            var obj = await ResourceManager.LoadAssetAsync<GameObject>(data.Prefab, ResType.Prefab);
            GamePlayEnemy e = obj.GetComponent<GamePlayEnemy>();
            var aie = e as GamePlayAIEntity;
            if (aie != null)
            {
                aie.graphPath = data.Graph;
                aie.pauseWhenInvisible = spawnedEnemyPauseWhenInvisible;
            }
            return e;
        }
        return null;
    }

#if UNITY_EDITOR
    protected override async UniTask TryLoadPreviewPrefab()
    {
        if (spawnEntityTableId > 0)
        {
            var data = EnemyTable.GetTableData(spawnEntityTableId);
            if (data != null)
            {
                previewPrefab = await ResourceManager.LoadAssetAsyncButNotInstance<GameObject>(data.Prefab, ResType.Prefab);
            }
        }
    }
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("Enemy³öÉúµã: ", GamePlayId);
        color = GamePlayId <= 0 ? Color.red : Color.yellow;
        return true;
    }
#endif
}
