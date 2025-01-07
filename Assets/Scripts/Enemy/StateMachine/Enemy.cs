using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Enemy : Entity
{

    [Header("Move Info")]
    public float moveSpeed = 3f;
    public float jumpForce = 12f;

    private Transform playerTrans;

    public EnemyStateMachine stateMachine { get; private set; }


    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();

    }

    protected override void Start()
    {
        base.Start();

        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected override void Update()
    {
        base.Update();

        stateMachine.currentState.Update();
    }
    public virtual bool IsPlayerExist()
    {
        if(playerTrans != null)
        {
            return true;
        }
        return false;
    }

    public virtual int RelativePosition()//与player的相对位置
    {
        if (this.transform.position.x - playerTrans.position.x < 0)
        {
            return 1;
        }
        else if (this.transform.position.x - playerTrans.position.x > 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

}
