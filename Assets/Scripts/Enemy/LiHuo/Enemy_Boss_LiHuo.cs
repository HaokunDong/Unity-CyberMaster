using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Boss_LiHuo : Enemy
{

    #region States

    public LiHuoIdleState idleState { get; private set; }
    public LiHuoMoveState moveState { get; private set; }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        idleState = new LiHuoIdleState(stateMachine, this, "Idle", this);
        moveState = new LiHuoMoveState(stateMachine, this, "Move", this);
    }


    protected override void Start()
    {
        base.Start();

        this.moveSpeed = base.moveSpeed * 1.5f;

        Flip();

        stateMachine.Initialize(idleState);

    }

    protected override void Update()
    {
        base.Update();

    }
}
