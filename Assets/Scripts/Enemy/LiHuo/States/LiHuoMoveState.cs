using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoMoveState : EnemyState
{
    private Enemy_Boss_LiHuo liHuo;

    public LiHuoMoveState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) : base(_stateMachine, _enemy, _animBoolName)
    {
        this.liHuo = _liHuo;
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

        liHuo.SetVelocity(liHuo.facingDir * liHuo.moveSpeed, liHuo.rb.velocity.y);

        if (liHuo.facingDir != liHuo.RelativePosition())
        {
            stateMachine.ChangeState(liHuo.idleState);
        }
    }
}
