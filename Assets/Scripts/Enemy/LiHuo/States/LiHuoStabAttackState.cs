using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoStabAttackState : EnemyState
{
    protected Enemy_Boss_LiHuo liHuo;
    public LiHuoStabAttackState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) : base(_stateMachine, _enemy, _animBoolName)
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
            stateTimer = 0.2f;
            liHuo.SetVelocity(liHuo.facingDir * liHuo.stabAttackMoveSpeed, rb.velocity.y);

            eventsTriggerCalled = false;
        }

        if (triggerCalled)
        {
            stateMachine.ChangeState(liHuo.battleState);
        }
    }
}
