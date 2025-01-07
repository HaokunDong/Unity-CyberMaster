using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoIdleState : EnemyState
{
    private Enemy_Boss_LiHuo liHuo;

    public LiHuoIdleState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) : base(_stateMachine, _enemy, _animBoolName)
    {
        this.liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();

        liHuo.ZeroVelocity();

        stateTimer = 3f;

    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {   
        base.Update();

        if(stateTimer < 0)
        {
            liHuo.FlipController(liHuo.RelativePosition());
        }

        if (liHuo.IsPlayerExist() && stateTimer < 0)
        {
            stateMachine.ChangeState(liHuo.moveState);
        }
    }
}
