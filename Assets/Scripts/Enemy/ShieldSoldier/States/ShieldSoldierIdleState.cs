using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSoldierIdleState : EnemyState
{
    protected Enemy_ShieldSoldier shieldSoldier;
    public ShieldSoldierIdleState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_ShieldSoldier _shieldSoldier) : base(_stateMachine, _enemy, _animBoolName)
    {
        shieldSoldier = _shieldSoldier;
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
    }
}
