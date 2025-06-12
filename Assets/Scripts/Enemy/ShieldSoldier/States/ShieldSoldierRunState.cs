using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSoldierRunState : EnemyState
{
    protected Enemy_ShieldSoldier shieldSoldier;
    public ShieldSoldierRunState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_ShieldSoldier _shieldSoldier) : base(_stateMachine, _enemy, _animBoolName)
    {
        shieldSoldier = _shieldSoldier;
    }

    public override void Enter()
    {
        base.Enter();

        if(shieldSoldier.facingDir != shieldSoldier.RelativePosition())
        {
            shieldSoldier.FlipController(shieldSoldier.RelativePosition());
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (shieldSoldier.IsPlayerInAttackRange())
        {
            stateMachine.ChangeState(shieldSoldier.primaryAttackState);
        }
    }
}
