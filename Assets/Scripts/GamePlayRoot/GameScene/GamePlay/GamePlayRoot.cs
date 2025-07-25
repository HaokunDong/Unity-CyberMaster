using Cinemachine;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Everlasting.Extend;
using GameBase.Log;
using GameScene.FlowNode;
using GameScene.FlowNode.Base;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tools;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;

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

    LevelLink = 90000, //关卡链接点
}

public class GamePlayRoot : MonoBehaviour, ICustomHierarchyComment
{
    public static GamePlayRoot Current = null;

    public bool inited = false;

    public bool IsActive
    {
        get => gameObject.activeInHierarchy;
        set
        {
            if (value)
            {
                StopTask();
                gameObject.SetActive(true);
                RunTask().Forget();
            }
            else
            {
                StopTask();
                gameObject.SetActive(false);
            }
        }
    }

    private void StopTask()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        task?.Stop();
        task = null;
    }

    private async UniTask RunTask()
    {
        await UniTask.WaitUntil(() => inited);

        cts = new CancellationTokenSource();

        task = new RepeatingTask(
            intervalInSeconds: 0.3f,
            action: async () =>
            {
                CheckAllTriggerAndDistanceSpawn();
            },
            externalToken: cts.Token,
            initialDelayInSeconds: 1.0f,
            continueCondition: () => this.isActiveAndEnabled,
            onCompleted: () => LogUtils.Warning("任务完成或中断")
        );

        task.Start();
    }

    [ReadOnly]
    public uint RootId;

    [NonSerialized, ReadOnly, ShowInInspector]
    public GamePlayPlayer player = null;

    private const uint GAP_L = 100000;
    private const uint GAP = 1000;

    [ShowInInspector]
    private Dictionary<Type, GamePlayEntityParent> entityParents;
    [ShowInInspector]
    private Dictionary<uint, GamePlayEnemy> enemyDict = null;
    [ShowInInspector]
    private Dictionary<uint, GamePlayNPC> NPCDict = null;
    [ShowInInspector]
    private Dictionary<uint, GamePlayItem> itemDict = null;
    [ShowInInspector]
    private Dictionary<uint, GamePlayTrigger> triggerDict = null;
    [ShowInInspector]
    private Dictionary<uint, GamePlaySpawnPoint> spawnPointDict = null;
    [ShowInInspector]
    private Dictionary<uint, GamePlayLevelLink> linkDict = null;

    private RepeatingTask task;
    private CancellationTokenSource cts;
    private IInteractable currentInteractTarget;
    public IInteractable InteractTarget => currentInteractTarget;
    private IInteractable lastInteractTarget;

    private CinemachineVirtualCamera virtualCamera;

    public async UniTask Init()
    {
        inited = false;
        entityParents = new Dictionary<Type, GamePlayEntityParent>();
        var ps = transform.GetComponentsInChildren<GamePlayEntityParent>(true);
        foreach(var p in ps)
        {
            entityParents[p.type] = p;
        }
        CollectDict(ref enemyDict);
        CollectDict(ref NPCDict);
        CollectDict(ref itemDict);
        CollectDict(ref triggerDict);
        CollectDict(ref spawnPointDict);
        CollectDict(ref linkDict);
        Current = this;

        FlowCtl?.Init();
        virtualCamera = Camera.main?.GetComponent<CinemachineBrain>()?.ActiveVirtualCamera as CinemachineVirtualCamera;

        await UniTask.DelayFrame(1);

        foreach (var kv in spawnPointDict)
        {
            var sp = kv.Value;
            if (sp.timing == SpawnTiming.AfterInit)
            {
                sp.Spawn().Forget();
            }
        }
        inited = true;
        IsActive = true;
    }

    private void CheckAllTriggerAndDistanceSpawn()
    {
        if(triggerDict != null)
        {
            foreach (var trigger in triggerDict.Values)
            {
                trigger.Check();
            }
        }

        if(player != null && spawnPointDict != null)
        {
            foreach (var sp in spawnPointDict.Values)
            {
                if(sp.timing == SpawnTiming.DistanceCloseEnough && sp.CheckCanSpawn())
                {
                    if(sp.needRaycastDetect)
                    {
                        Vector2 direction = (player.transform.position - sp.transform.position).normalized;
                        var hit = Physics2D.Raycast(sp.transform.position, direction, sp.spawnDistance, LayerMask.GetMask("Player"));
                        if(hit.collider != null)
                        {
                            sp.Spawn().Forget();
                        }
                    }
                    else
                    {
                        float distance = Vector3.Distance(sp.transform.position, player.transform.position);
                        if(distance <= sp.spawnDistance)
                        {
                            sp.Spawn().Forget();
                        }
                    }
                }
            }
        }

        DetectClosestInteractable();
        CheckVisibleAIs();
    }

    private void DetectClosestInteractable()
    {
        lastInteractTarget = currentInteractTarget; // 保存上一次的

        currentInteractTarget = null;

        if (player == null) return;

        Vector2 origin = player.transform.position;
        Vector2 facing = player.GetFacingDirection().normalized;
        float maxDistance = player.maxInteractDistance;
        int layerMask = LayerMask.GetMask("Interactable", "NPC");

        // 先检测前方
        var frontHits = Physics2D.RaycastAll(origin, facing, maxDistance, layerMask);

        float closestDistance = float.MaxValue;
        foreach (var hit in frontHits)
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null && interactable.canInteract)
            {
                float dist = Vector2.Distance(origin, hit.point);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    currentInteractTarget = interactable;
                }
            }
        }

        //再检测身后
        if(currentInteractTarget == null)
        {
            var backHits = Physics2D.RaycastAll(origin, -facing, maxDistance, layerMask);
            foreach (var hit in backHits)
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.canInteract)
                {
                    float dist = Vector2.Distance(origin, hit.point);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        currentInteractTarget = interactable;
                    }
                }
            }
        }

        if (currentInteractTarget != lastInteractTarget)
        {
            OnInteractTargetChanged(lastInteractTarget, currentInteractTarget);
        }
    }

    private void OnInteractTargetChanged(IInteractable oldTarget, IInteractable newTarget)
    {
        // 可以取消旧目标高亮
        if (oldTarget != null)
        {
            InteractHUD.Instance.SetTarget(null);
        }

        // 可以激活新目标提示
        if (newTarget != null)
        {
            InteractHUD.Instance.SetTarget(newTarget.Transform);
        }
    }

    private void CheckVisibleAIs()
    {
        if (player == null || (enemyDict == null && NPCDict == null)) return;

        float cameraSize = virtualCamera != null ? virtualCamera.m_Lens.OrthographicSize : Camera.main.orthographicSize;
        float activeRange = 2 * cameraSize * 1.3f;
        Vector2 playerPos = player.transform.position;

        if(enemyDict != null)
        {
            foreach (var kv in enemyDict)
            {
                var enemy = kv.Value;
                if (enemy == null) continue;

                float dist = Vector2.Distance(playerPos, enemy.transform.position);
                bool shouldBeActive = dist <= activeRange;

                enemy.SetAIActive(shouldBeActive);
            }
        }
        
        if(NPCDict != null)
        {
            foreach (var kv in NPCDict)
            {
                var npc = kv.Value;
                if (npc == null) continue;

                float dist = Vector2.Distance(playerPos, npc.transform.position);
                bool shouldBeActive = dist <= activeRange;

                npc.SetAIActive(shouldBeActive);
            }
        }
    }

    public void SendGamePlayMsg<M>(M msg) where M : IFlowMessage
    {
        flowCtl?.SendFlowMessage(msg);
    }

    public async UniTask SendGamePlayAsyncMsg<M>(M msg) where M : IFlowAsyncMessage
    {
        if(flowCtl != null)
        {
            await flowCtl.SendFlowMessageAsync(msg);
        }
    }

    public T GetAGamePlayEntity<T>(uint GamePlayId) where T : GamePlayEntity
    {
        if (GamePlayId <= 0)
        {
            return null;
        }
        if (typeof(GamePlayEnemy).IsAssignableFrom(typeof(T)) && enemyDict != null && enemyDict.TryGetValue(GamePlayId, out var enemy))
        {
            return enemy as T;
        }
        else if (typeof(GamePlayNPC).IsAssignableFrom(typeof(T)) && NPCDict != null && NPCDict.TryGetValue(GamePlayId, out var npc))
        {
            return npc as T;
        }
        else if (typeof(GamePlayItem).IsAssignableFrom(typeof(T)) && itemDict != null && itemDict.TryGetValue(GamePlayId, out var item))
        {
            return item as T;
        }
        else if (typeof(GamePlayTrigger).IsAssignableFrom(typeof(T)) && triggerDict != null && triggerDict.TryGetValue(GamePlayId, out var trigger))
        {
            return trigger as T;
        }
        else if (typeof(GamePlaySpawnPoint).IsAssignableFrom(typeof(T)) && spawnPointDict != null && spawnPointDict.TryGetValue(GamePlayId, out var spawnPoint))
        {
            return spawnPoint as T;
        }
        else if (typeof(GamePlayLevelLink).IsAssignableFrom(typeof(T)) && linkDict != null && linkDict.TryGetValue(GamePlayId, out var levelLink))
        {
            return levelLink as T;
        }
        return null;
    }

    public async UniTask<GamePlayEntity> DoGamePlaySpawn(uint SpawnPointGamePlayId)
    {
        if(spawnPointDict != null && spawnPointDict.TryGetValue(SpawnPointGamePlayId, out var spawnPoint))
        {
            var entity = await spawnPoint.Spawn();
            return entity;
        }
        return null;
    }

    public void AfterAnEntitySpawned<T>(T spawnedEntity, GamePlaySpawnPoint gamePlaySpawnPoint) where T : GamePlayEntity
    {
        spawnedEntity.transform.SetPositionAndRotation(gamePlaySpawnPoint.transform.position, gamePlaySpawnPoint.transform.rotation);
        if (Quaternion.Angle(gamePlaySpawnPoint.transform.rotation, Quaternion.identity) > 90)
        {
            spawnedEntity.FlipData();
        }
        spawnedEntity.isGen = true;
        spawnedEntity.spawnPoint = gamePlaySpawnPoint;
        var index = gamePlaySpawnPoint.GamePlayId % GAP;
        if(spawnedEntity is GamePlayPlayer player)
        {
            this.player = player;
            player.transform.SetParent(entityParents[typeof(GamePlayPlayer)].transform);
            player.Init();
            currentInteractTarget = null;
            if(virtualCamera != null)
            {
                virtualCamera.Follow = player.transform;
            }
        }
        else if (spawnedEntity is GamePlayEnemy enemy)
        {
            var id = RootId * GAP_L + (uint)GamePlayIdBegin.Enemy_Gen + index;
            enemyDict[id] = enemy;
            enemy.GamePlayId = id;
            enemy.transform.SetParent(entityParents[typeof(GamePlayEnemy)].transform);
        }
        else if(spawnedEntity is GamePlayNPC NPC)
        {
            var id = RootId * GAP_L + (uint)GamePlayIdBegin.NPC_Gen + index;
            NPCDict[id] = NPC;
            NPC.GamePlayId = id;
            NPC.transform.SetParent(entityParents[typeof(GamePlayNPC)].transform);
        }
        else if (spawnedEntity is GamePlayItem item)
        {
            var id = RootId * GAP_L + (uint)GamePlayIdBegin.Item_Gen + index;
            itemDict[id] = item;
            item.GamePlayId = id;
            item.transform.SetParent(entityParents[typeof(GamePlayItem)].transform);
        }
        spawnedEntity.Init();
    }

    private void CollectDict<T>(ref Dictionary<uint, T> dict) where T : GamePlayEntity
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
            entity.Init();
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
            var checks = instance.transform.GetComponentsInChildren<GamePlayEntityParent>();
            foreach(var c in checks)
            {
                c.Check();
            }

            var root = instance.GetComponent<GamePlayRoot>();
            if(root != null)
            {
                var GamePlayId = root.RootId;
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
                        var links = instance.transform.GetComponentsInChildren<GamePlayLevelLink>();
                        GenId(links, GamePlayId, GamePlayIdBegin.LevelLink);
                    }

                    s_needParseGamePlayRoot = false;
                }
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
            RootId = table.Id;
        }
        else
        {
            RootId = 0;
        }
        name = ZString.Concat("关卡根节点", " id: ", RootId);
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
