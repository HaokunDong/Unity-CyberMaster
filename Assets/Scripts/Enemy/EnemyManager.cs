using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : SingletonComp<EnemyManager>
{
    public Enemy enemy;
    public Enemy_Boss_LiHuo liHuo;

    private void Start()
    {
        //liHuo.gameObject.SetActive(false);
    }

}
