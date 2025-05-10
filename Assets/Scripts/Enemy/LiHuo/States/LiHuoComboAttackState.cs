using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoComboAttackState : EnemyState
{
    protected Enemy_Boss_LiHuo liHuo;
    public LiHuoComboAttackState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo)
        : base(_stateMachine, _enemy, _animBoolName)
    {
        liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();
        liHuo.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (liHuo.IsPlayerInAttackRange())
        {
            stateMachine.ChangeState(liHuo.primaryAttackState);
        }
    }
}
