using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Boss_LiHuo : Enemy
{

    #region States

    public LiHuoIdleState idleState { get; private set; }

    #endregion


    protected override void Awake()
    {
        base.Awake();

        idleState = new LiHuoIdleState(stateMachine, this, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
    }
}
