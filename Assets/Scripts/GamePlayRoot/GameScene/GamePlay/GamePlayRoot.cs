using Cysharp.Text;
using Everlasting.Config;
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
}

public class GamePlayRoot : MonoBehaviour, ICustomHierarchyComment
{
    [ReadOnly]
    public uint GamePlayId;

    private const uint GAP_L = 100000;
    private const uint GAP = 1000;

    public void Init()
    {
        FlowCtl?.Init();
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
                    var gpes = instance.transform.GetComponentsInChildren<GamePlayEnemy>();
                    GenId(gpes, GamePlayId, GamePlayIdBegin.Enemy);
                    var gpns = instance.transform.GetComponentsInChildren<GamePlayNPC>();
                    GenId(gpns, GamePlayId, GamePlayIdBegin.NPC);

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
