using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using GameBase.Log;
using GameScene.FlowNode;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

public enum GamePlayIdBegin
{
    SpawnPoint = 1000,
    Trigger = 2000,
    NPC = 30000,
    NPC_Gen = 31000,
    Enemy = 40000,
    Enemy_Gen = 41000,
    Item = 50000,
    Item_Gen = 51000,
}

public class GamePlayRoot : MonoBehaviour, ICustomHierarchyComment
{
    public static GamePlayRoot Current = null;

    [ReadOnly]
    public uint GamePlayId;

    private const uint GAP_L = 100000;
    private const uint GAP = 1000;

    private Dictionary<uint, GamePlayEnemy> enemyDict = null;
    private Dictionary<uint, GamePlayNPC> NPCDict = null;
    private Dictionary<uint, GamePlayItem> itemDict = null;
    private Dictionary<uint, GamePlayTrigger> triggerDict = null;
    private Dictionary<uint, GamePlaySpawnPoint> spawnPointDict = null;

    public void Init()
    {
        if (Current != this)
        {
            Current = this;
        }
        CollectDict(enemyDict);
        CollectDict(NPCDict);
        CollectDict(itemDict);
        CollectDict(triggerDict);
        CollectDict(spawnPointDict);
        FlowCtl?.Init();
    }

    public T GetAGamePlayEntity<T>(uint GamePlayId) where T : GamePlayEntity
    {
        if (GamePlayId <= 0)
        {
            return null;
        }
        if (typeof(T) == typeof(GamePlayEnemy) && enemyDict != null && enemyDict.TryGetValue(GamePlayId, out var enemy))
        {
            return enemy as T;
        }
        else if (typeof(T) == typeof(GamePlayNPC) && NPCDict != null && NPCDict.TryGetValue(GamePlayId, out var npc))
        {
            return npc as T;
        }
        else if (typeof(T) == typeof(GamePlayItem) && itemDict != null && itemDict.TryGetValue(GamePlayId, out var item))
        {
            return item as T;
        }
        else if (typeof(T) == typeof(GamePlayTrigger) && triggerDict != null && triggerDict.TryGetValue(GamePlayId, out var trigger))
        {
            return trigger as T;
        }
        else if (typeof(T) == typeof(GamePlaySpawnPoint) && spawnPointDict != null && spawnPointDict.TryGetValue(GamePlayId, out var spawnPoint))
        {
            return spawnPoint as T;
        }
        return null;
    }

    private void CollectDict<T>(Dictionary<uint, T> dict) where T : GamePlayEntity
    {
        if (dict == null)
        {
            dict = new Dictionary<uint, T>();
        }
        else
        {
            dict.Clear();
        }
        var entities = transform.GetComponentsInChildren<T>(true);
        foreach (var entity in entities)
        {
            if (entity.GamePlayId > 0)
            {
                dict[entity.GamePlayId] = entity;
            }
        }
    }

    private GamePlayFlowController flowCtl = null;
    private GamePlayFlowController FlowCtl
    {
        get
        {
            if(flowCtl == null)
            {
                flowCtl = GetComponent<GamePlayFlowController>();
            }
            return flowCtl;
        }
    }

    public async UniTask Dispose()
    {
        if(Current == this)
        {
            Current = null;
        }

        await UniTask.DelayFrame(1);
    }

#if UNITY_EDITOR
    private static bool s_needParseGamePlayRoot = false;
    private static bool TableLoaded;

    [InitializeOnLoadMethod]
    private static async void LoadTable()
    {
        TableDataManager.Clear();
        TableLoaded = false;
        var gameFacadeExist = UnityEngine.Object.FindObjectOfType<GameFacade>() != null;
        if (EditorApplication.isPlayingOrWillChangePlaymode && gameFacadeExist)
        {
            TableLoaded = true;
            return;
        }

        try
        {
            await TableDataManager.Load();
            TableLoaded = true;
        }
        catch (Exception e)
        {
            TableLoaded = false;
            Debug.LogError(e);
        }
    }

    //自动保存
    [InitializeOnLoadMethod]
    private static void StartInitializeOnLoadMethod()
    {
        //坑，从一个prefab模式下打开另一个prefab时，会先触发prefabStageOpened再触发prefabStageClosing
        //这个如果编辑gameplay所在prefab不保存，会导致下次非gameplay的prefab保存时多遍历一遍，但其实影响不大
        PrefabStage.prefabSaving += delegate (GameObject instance)
        {
            var checks = instance.transform.GetComponentsInChildren<GamePlayEntityCheck>();
            foreach(var c in checks)
            {
                c.Check();
            }

            var GamePlayId = instance.GetComponent<GamePlayRoot>().GamePlayId;
            if (GamePlayId > 0)
            {
                if (s_needParseGamePlayRoot)
                {
                    var gamePlayEnemies = instance.transform.GetComponentsInChildren<GamePlayEnemy>();
                    GenId(gamePlayEnemies, GamePlayId, GamePlayIdBegin.Enemy);
                    var gamePlayNPCs = instance.transform.GetComponentsInChildren<GamePlayNPC>();
                    GenId(gamePlayNPCs, GamePlayId, GamePlayIdBegin.NPC);
                    var gamePlayItems = instance.transform.GetComponentsInChildren<GamePlayItem>();
                    GenId(gamePlayItems, GamePlayId, GamePlayIdBegin.Item);
                    var gamePlayTriggers = instance.transform.GetComponentsInChildren<GamePlayTrigger>();
                    GenId(gamePlayTriggers, GamePlayId, GamePlayIdBegin.Trigger);
                    var gamePlaySpawnPoints = instance.transform.GetComponentsInChildren<GamePlaySpawnPoint>();
                    GenId(gamePlaySpawnPoints, GamePlayId, GamePlayIdBegin.SpawnPoint);
                }

                s_needParseGamePlayRoot = false;
            }
        };
    }

    private static void GenId(GamePlayEntity[] list, uint GPID, GamePlayIdBegin begin)
    {
        uint next = 0;
        HashSet<uint> ids = new();
        HashSet<uint> finished = new();
        foreach (var e in list)
        {
            if (e.GamePlayId / GAP_L == GPID && e.GamePlayId % GAP_L > (uint)begin && e.GamePlayId % GAP_L < (uint)begin + GAP)
            {
                ids.Add(e.GamePlayId);
                next = Math.Max(next, e.GamePlayId + 1);
            }
        }
        next = Math.Max(next, GPID * GAP_L + (uint)begin + 1);
        foreach (var e in list)
        {
            if (finished.Contains(e.GamePlayId) || e.GamePlayId / GAP_L != GPID || e.GamePlayId % GAP_L <= (uint)begin || e.GamePlayId % GAP_L >= (uint)begin + GAP)
            {
                while(ids.Contains(next))
                {
                    next++;
                }
                e.GamePlayId = next;
                ids.Add(next);
                next++;
            }
            finished.Add(e.GamePlayId);
        }
        list.Sort((a, b) => a.GamePlayId.CompareTo(b.GamePlayId));
        for (int i = 0; i < list.Length; i++)
        {
            list[i].transform.SetSiblingIndex(i);
        }
    }

    public bool GetHierarchyComment(out string name, out Color color)
    {
        GamePlayTable table = null;
        if (TableLoaded)
        {
            table = GamePlayTable.All.FirstOrDefault(t => t.Prefab.Contains(gameObject.name));
        }
        if(table != null)
        {
            GamePlayId = table.Id;
        }
        else
        {
            GamePlayId = 0;
        }
        name = ZString.Concat("关卡根节点", " id: ", GamePlayId);
        color = Color.green;
        return true;
    }

    private void OnValidate()
    {
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            s_needParseGamePlayRoot = true;
        }
    }
#endif
}
