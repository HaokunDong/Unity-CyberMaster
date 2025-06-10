using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using System;
using UnityEngine;

public class PlayerManager : SingletonComp<PlayerManager>
{
    [HideInInspector, NonSerialized]
    public Player player = null;
    public GameObject playerPrefab;
    public string playerCreateAnim;

    public Player CreatePlayer(GameObject playerSpawnPoint)
    {
        if (player != null)
        {
            return player;
        }
        if (player == null && playerPrefab != null)
        {
            var obj = GameObject.Instantiate(playerPrefab, playerSpawnPoint.transform.position, playerSpawnPoint.transform.rotation);
            player = obj.GetComponent<Player>();
            if(Quaternion.Angle(playerSpawnPoint.transform.rotation, Quaternion.identity) > 90)
            {
                player.FlipData();
            }
            if (!playerCreateAnim.IsNullOrEmpty())
            {
                PlayCreateAnim(playerCreateAnim).Forget();
            }
            return player;
        }
        return null;
    }

    private async UniTask PlayCreateAnim(string name)
    {
        await UniTask.WaitUntil(() => player != null && player.animator != null);
        player.animator.Play(name);
    }
}
