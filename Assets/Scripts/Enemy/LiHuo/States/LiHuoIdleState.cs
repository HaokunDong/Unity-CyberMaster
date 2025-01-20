using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoIdleState : LiHuoGroundState
{
    public LiHuoIdleState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) : base(_stateMachine, _enemy, _animBoolName, _liHuo)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = 1f;

        liHuo.SetZeroVelocity();

    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {   
        base.Update();

        if (liHuo.IsPlayerExist() && liHuo.facingDir == liHuo.RelativePosition() && stateTimer < 0)
        {
            stateMachine.ChangeState(liHuo.moveState);
        }

        if (liHuo.facingDir != liHuo.RelativePosition() && stateTimer < 0)
        {
            liHuo.FlipController(liHuo.RelativePosition());
        }

    }
}
