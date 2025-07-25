using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : SingletonComp<World>
{
    [NonSerialized, ShowInInspector, ReadOnly]
    private Dictionary<uint, GamePlayRoot> GamePlayRootOnWorld = new ();
    [NonSerialized, ShowInInspector, ReadOnly]
    private HashSet<uint> InPipelineWorldIds = new();

    public async UniTask BeginLoad(uint RootId, uint From = 0)
    {
        if (!InPipelineWorldIds.Contains(RootId))
        {
            InPipelineWorldIds.Add(RootId);
            var gpObj = await ResourceManager.LoadAssetAsync<GameObject>(GamePlayTable.GetTableData(RootId).Prefab, ResType.Prefab);
            if(From > 0)
            {

            }
            var gp = gpObj.GetComponent<GamePlayRoot>();
            GamePlayRootOnWorld[RootId] = gp;
            await gp.Init();
        }
    }
}
