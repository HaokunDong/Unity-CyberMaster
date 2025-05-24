using Cysharp.Threading.Tasks;
using Everlasting.Config;
using GameBase.Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : SingletonComp<PlayerManager>
{
    public Player player;

    private void Start()
    {
        TableTest().Forget();
    }

    public static async UniTask TableTest()
    {
        await TableDataManager.Load();
        var data = EnemyTable.GetTableData(20001);
        LogUtils.Error(data.Id, LogChannel.Load, Color.red);
        LogUtils.Error(data.EnemyName, LogChannel.Load, Color.green);
        LogUtils.Error(data.EnemyHP, LogChannel.Load, Color.yellow);
    }
}
