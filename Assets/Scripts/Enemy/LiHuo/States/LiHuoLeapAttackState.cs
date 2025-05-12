using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoLeapAttackState : EnemyState
{
    protected Enemy_Boss_LiHuo liHuo;
    public LiHuoLeapAttackState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) : base(_stateMachine, _enemy, _animBoolName)
    {
        liHuo = _liHuo;
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

        if (stateTimer < 0)
        {
            liHuo.SetZeroVelocity();
        }

        if (eventsTriggerCalled)
        {
            stateTimer = liHuo.RelativeDistance() / liHuo.leapAttackMoveSpeed;
            liHuo.SetVelocity(liHuo.facingDir * liHuo.leapAttackMoveSpeed, rb.velocity.y);

            eventsTriggerCalled = false;
        }

        if (triggerCalled)
        {
            stateMachine.ChangeState(liHuo.battleState);
        }
    }
}
