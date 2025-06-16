using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;

public class GamePlayPlayerSpawnPoint : GamePlaySpawnPoint<GamePlayPlayer>
{
    public override bool CheckCanSpawn()
    {
        return !spawned;
    }

    public override async UniTask<GamePlayPlayer> Spawn<GamePlayPlayer>()
    {
        var obj = await Managers.ResourceManager.LoadAssetAsync<GameObject>("Player/Player", ResType.Prefab);
        GamePlayPlayer p = obj.GetComponent<GamePlayPlayer>();
        return p;
    }

#if UNITY_EDITOR

    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("Player³öÉúµã: ", GamePlayId);
        color = Color.green;
        return true;
    }
#endif
}
