using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoSpawner : SingletonComp<LiHuoSpawner>
{
    [Header("Boss Prefabs")]
    [SerializeField] public GameObject liHuoPrefab;

    [Header("Spawn Settings")]
    [SerializeField] public Transform liHuoSpawnPoint;
    private bool hasSpawned = false;

    [Header("Trigger Spawn")]
    [SerializeField] public float triggerDistance; // 玩家距离多少时生成

    private BoxCollider2D collider2D;

    //private void Awake()
    //{
    //    collider2D = GetComponent<BoxCollider2D>();
    //    collider2D.isTrigger = true;
    //}

    protected override void OnSingletonInit()
    {
        collider2D = GetComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D player)
    {
        async UniTask CreateBoss()
        {
            var boss = EnemyManager.Ins.CreateEnemy_Boss_LiHuo(this);
            boss.gameObject.SetActive(true);
            await UniTask.DelayFrame(1);
            boss.stateMachine.ChangeState(EnemyManager.Ins.liHuo.appearState);
            hasSpawned = true;
            Destroy(this);
        }
        if (player.CompareTag("Player"))
        {
            CreateBoss().Forget();
        }
    }

    public void SpawnBoss()
    {
        if (liHuoPrefab == null)
        {
            Debug.LogError("Boss Prefab 未分配！");
            return;
        }

        if (liHuoSpawnPoint == null)
        {
            Debug.LogError("Boss 生成点未设置！");
            return;
        }

        // 实例化Boss
        //currentBoss = Instantiate(liHuoPrefab, liHuoSpawnPoint.position, liHuoSpawnPoint.rotation);
        //currentBoss.name = ""; // 可选：重命名实例
        Debug.Log("Boss 已生成！");
    }
}
