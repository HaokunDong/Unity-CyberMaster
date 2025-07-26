using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class World : SingletonComp<World>
{
    [ShowInInspector, ReadOnly]
    public GamePlayPlayer Player { get; private set; }
    [ShowInInspector, ReadOnly]
    public GamePlayRoot InPlayGamePlayRoot { get; private set; }

    [NonSerialized, ShowInInspector, ReadOnly]
    private Dictionary<uint, GamePlayRoot> GamePlayRootOnWorld = new ();
    [NonSerialized, ShowInInspector, ReadOnly]
    private HashSet<uint> InPipelineWorldIds = new();

    public async UniTask<GamePlayRoot> BeginLoad(uint RootId, uint From, string gateName, Vector3 worldPos)
    {
        if (!InPipelineWorldIds.Contains(RootId))
        {
            InPipelineWorldIds.Add(RootId);
            var gpObj = await ResourceManager.LoadAssetAsync<GameObject>(GamePlayTable.GetTableData(RootId).Prefab, ResType.Prefab);
            var gp = gpObj.GetComponent<GamePlayRoot>();
            GamePlayRootOnWorld[RootId] = gp;
            await gp.Init(From, gateName, worldPos);
        }
        GamePlayRootOnWorld.TryGetValue(RootId, out var res);
        return res;
    }

    public void SetPlayer(GamePlayPlayer p)
    {
        if(Player == null)
        {
            Player = p;
            Player.transform.SetParent(transform);
        }
    }

    public void PlayerEnterGamePlayRoot(GamePlayRoot root)
    {
        InPlayGamePlayRoot = root;
    }

    public GamePlayRoot GetRootByRootId(uint id)
    {
        GamePlayRootOnWorld.TryGetValue(id, out var root);
        return root;
    }

    public GamePlayRoot GetRootByEntityId(uint eid)
    {
        var id = eid / GamePlayRoot.GAP_L;
        GamePlayRootOnWorld.TryGetValue(id, out var root);
        return root;
    }
}
