using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : SingletonComp<EnemyManager>
{
    [HideInInspector]
    public Enemy enemy;
    [HideInInspector]
    public Enemy_Boss_LiHuo liHuo;

    public GameObject boss_LiHuoPrefab;

    public Enemy_Boss_LiHuo CreateEnemy_Boss_LiHuo(LiHuoSpawner spawnPoint)
    {
        if (liHuo != null)
        {
            return liHuo;
        }
        if (liHuo == null && boss_LiHuoPrefab != null)
        {
            var obj = GameObject.Instantiate(boss_LiHuoPrefab, spawnPoint.transform.position, Quaternion.identity);
            liHuo = obj.GetComponent<Enemy_Boss_LiHuo>();
            return liHuo;
        }
        return null;
    }

}
