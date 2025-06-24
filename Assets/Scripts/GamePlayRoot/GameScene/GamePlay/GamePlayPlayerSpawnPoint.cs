using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using Managers;
using UnityEngine;
using UnityEngine.Playables;

public class GamePlayPlayerSpawnPoint : GamePlaySpawnPoint<GamePlayPlayer>
{
    public string playerCreateAnim = null;

    public override bool CheckCanSpawn()
    {
        return !spawned;
    }

    public override async UniTask<GamePlayPlayer> Spawn<GamePlayPlayer>()
    {
        var obj = await Managers.ResourceManager.LoadAssetAsync<GameObject>("Player/Player", ResType.Prefab);
        GamePlayPlayer p = obj.GetComponent<GamePlayPlayer>();
        //临时兼容老Player处理
        if (Quaternion.Angle(transform.rotation, Quaternion.identity) > 90)
        {
            var player = obj.GetComponent<Player>();
            player.FlipData();
        }
        //临时兼容老Player处理
        if (!playerCreateAnim.IsNullOrEmpty())
        {
            var animator = obj.GetComponentInChildren<Animator>();
            if(animator != null)
            {
                animator.Play(playerCreateAnim);
            }
        }
        return p;
    }

#if UNITY_EDITOR

    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("Player出生点: ", GamePlayId);
        color = Color.green;
        return true;
    }
#endif
}
