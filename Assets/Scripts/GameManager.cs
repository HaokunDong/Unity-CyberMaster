using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : SingletonComp<GameManager>
{
    public Transform player;

    [SerializeField] public float interactionDistance = 10.0f;//

    [Header("Boss Prefabs")]
    [SerializeField] public GameObject liHuoPrefab;

    [Header("Spawn Settings")]
    [SerializeField] public Transform liHuoSpawnPoint;
    private bool hasSpawned = false;

    [Header("Trigger Spawn")]
    [SerializeField] public float triggerDistance; // 玩家距离多少时生成

    public GameObject currentBoss;

    public void Start()
    {
        //player = PlayerManager.Ins.player.transform;

    }

    public void OnUpdate()
    {
        if (Vector2.Distance(PlayerManager.Ins.player.transform.position, EnemyManager.Ins.liHuo.transform.position) < 10 && !hasSpawned)
        {
            EnemyManager.Ins.liHuo.gameObject.SetActive(true);
            EnemyManager.Ins.liHuo.stateMachine.ChangeState(EnemyManager.Ins.liHuo.appearState);
            hasSpawned = true;
        }

        /*if (player != null && !hasSpawned)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= triggerDistance)
            {
                SpawnBoss();
                hasSpawned = true;
            }
        }*/
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
        currentBoss = Instantiate(liHuoPrefab, liHuoSpawnPoint.position, liHuoSpawnPoint.rotation);
        //currentBoss.name = ""; // 可选：重命名实例
        Debug.Log("Boss 已生成！");
    }

    public void DespawnBoss()
    {
        if (currentBoss != null)
        {
            Destroy(currentBoss);
            Debug.Log("Boss 已移除！");
        }
    }
}