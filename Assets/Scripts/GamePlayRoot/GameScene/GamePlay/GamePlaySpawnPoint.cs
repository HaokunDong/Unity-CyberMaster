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
#if UNITY_EDITOR
    [OnValueChanged("OnTableIdChanged")]
#endif
    public uint spawnEntityTableId;
    public SpawnTiming timing;
    [ShowIf("@timing == SpawnTiming.DistanceCloseEnough")]
    public bool needRaycastDetect = false;
    [ShowIf("@timing == SpawnTiming.DistanceCloseEnough")]
    public float spawnDistance = 10f;

    protected bool spawned = false;
    protected bool canSpawn = true;

    public abstract UniTask<GamePlayEntity> Spawn();

    public abstract UniTask<GamePlayEntity> SpawnEntity();

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
    protected GameObject previewPrefab;
    protected virtual void OnTableIdChanged()
    {
        previewPrefab = null;
    }

    protected virtual async UniTask TryLoadPreviewPrefab() { }
    protected virtual void OnDrawGizmosImip()
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

        if (previewPrefab == null)
        {
            TryLoadPreviewPrefab().Forget();
        }

        if (previewPrefab != null)
        {
            var spriteRenderer = previewPrefab.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;

            Texture2D texture = AssetPreview.GetAssetPreview(spriteRenderer.sprite);

            if (texture != null)
            {
                // 创建一个矩形用于绘制（投影到 SceneView）
                Handles.BeginGUI();

                float w = 150f;
                float h = 150f;

                Rect rect = new Rect(screenPos.x - w / 2, screenPos.y - h / 2, w, h);
                Color oldColor = GUI.color;
                GUI.color = new Color(1, 1, 1, 0.3f);
                GUI.DrawTexture(rect, texture);
                GUI.color = oldColor;
                Handles.EndGUI();
            }
        }
    }
    private void OnDrawGizmos()
    {
        OnDrawGizmosImip();
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
            spawnedEntity = await SpawnEntity() as T;
            spawnedEntity.TableId = spawnEntityTableId;
            spawned = true;
            World.Ins.GetRootByEntityId(GamePlayId)?.AfterAnEntitySpawned(spawnedEntity, this);
            AfterSpawn();
            return spawnedEntity;
        }
        return null;
    }

    public virtual void AfterSpawn() { }
}