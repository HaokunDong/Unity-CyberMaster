using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoMoveState : LiHuoBattleState
{
    public LiHuoMoveState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo)
        : base(_stateMachine, _enemy, _animBoolName, _liHuo)
    {
    }

    public override void Enter()
    {
        base.Enter();


    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        liHuo.SetVelocity(liHuo.facingDir * liHuo.moveSpeed, rb.velocity.y);

    }
}
