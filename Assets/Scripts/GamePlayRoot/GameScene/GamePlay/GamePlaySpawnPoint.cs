using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public enum SpawnTiming
{
    None = 0,
    AfterInit = 1,
    DistanceCloseEnough = 2,
}

public abstract class GamePlaySpawnPoint : GamePlayEntity
{
    public uint spawnEntityTableId;
    public SpawnTiming timing;
    [ShowIf("@timing == SpawnTiming.DistanceCloseEnough")]
    public bool needRaycastDetect = false;
    [ShowIf("@timing == SpawnTiming.DistanceCloseEnough")]
    public float spawnDistance = 10f;

    protected bool spawned = false;
    protected bool canSpawn = true;

    public abstract UniTask<GamePlayEntity> Spawn();

    public abstract UniTask<T> Spawn<T>() where T : GamePlayEntity;

    public virtual bool CheckCanSpawn()
    {
        canSpawn = spawnEntityTableId > 0 && !spawned && CustomCheck();
        return canSpawn;
    }

    public virtual bool CustomCheck()
    {
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        if (timing == SpawnTiming.DistanceCloseEnough)
        {
            Gizmos.color = needRaycastDetect ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnDistance);
        }

        Handles.BeginGUI();
        Vector3 worldPos = transform.position;
        Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);

        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.fontSize = 12;

        string text = timing switch
        {
            SpawnTiming.None => "被动出生点",
            SpawnTiming.AfterInit => "初始化后执行出生点",
            SpawnTiming.DistanceCloseEnough => "距离出生点",
            _ => "未知"
        };

        Color color = timing switch
        {
            SpawnTiming.None => Color.white,
            SpawnTiming.AfterInit => Color.green,
            SpawnTiming.DistanceCloseEnough => Color.yellow,
            _ => Color.red
        };
        style.normal.textColor = color;

        GUI.Label(new Rect(screenPos.x, screenPos.y, 200, 60), ZString.Concat(text, "\r\n", GamePlayId), style);
        Handles.EndGUI();
    }
#endif
}


public abstract class GamePlaySpawnPoint<T> : GamePlaySpawnPoint
    where T : GamePlayEntity
{
    [NonSerialized, ReadOnly, ShowInInspector]
    public T spawnedEntity;

    public sealed override async UniTask<GamePlayEntity> Spawn()
    {
        //先检查存档等条件决定是否要产生
        if(CheckCanSpawn())
        {
            spawnedEntity = await Spawn<T>();
            spawnedEntity.TableId = spawnEntityTableId;
            spawned = true;
            GamePlayRoot.Current.AfterAnEntitySpawned(spawnedEntity, this);
            AfterSpawn();
            return spawnedEntity;
        }
        return null;
    }

    public virtual void AfterSpawn() { }
}