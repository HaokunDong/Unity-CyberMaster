using Cysharp.Threading.Tasks;
using Everlasting.Config;
using GameBase.Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : SingletonComp<PlayerManager>
{
    public Player player;
    public GameObject playerPrefab;

    private void Start()
    {
        TableTest().Forget();
    }

    public Player CreatePlayer(GameObject playerSpawnPoint)
    {
        if (player != null)
        {
            return player;
        }
        if (player == null && playerPrefab != null)
        {
            var obj = GameObject.Instantiate(playerPrefab, playerSpawnPoint.transform.position, Quaternion.identity);
            player = obj.GetComponent<Player>();
            return player;
        }
        return null;
    }

    public static async UniTask TableTest()
    {
        await TableDataManager.Load();
        var data = EnemyTable.GetTableData(20001);
        LogUtils.Warning(data.Id, LogChannel.Load, Color.red);
        LogUtils.Error(data.EnemyName, LogChannel.Load, Color.green);
        LogUtils.Trace(data.EnemyHP, LogChannel.Load, Color.yellow);
    }
}
