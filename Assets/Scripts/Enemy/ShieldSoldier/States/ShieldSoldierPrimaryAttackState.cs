using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSoldierPrimaryAttackState : EnemyState
{
    protected Enemy_ShieldSoldier shieldSoldier;

    protected int comboCounter;
    protected float lastTimeAttacked;
    protected float comboWindow = 0.3f;

    public ShieldSoldierPrimaryAttackState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_ShieldSoldier _shieldSoldier) : base(_stateMachine, _enemy, _animBoolName)
    {
        shieldSoldier = _shieldSoldier;
    }

    public override void Enter()
    {
        base.Enter();

        if (comboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow)
        {
            comboCounter = 0;
        }

        shieldSoldier.attackCount = comboCounter;
        shieldSoldier.animator.SetInteger("ComboCounter", comboCounter);

        stateTimer = 0.1f;

        shieldSoldier.SetVelocity(shieldSoldier.attackMoveSpeed[comboCounter].x * shieldSoldier.facingDir, shieldSoldier.attackMoveSpeed[comboCounter].y);
    }

    public override void Exit()
    {
        base.Exit();

        comboCounter++;

        lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            shieldSoldier.SetZeroVelocity();
        }

        if (triggerCalled)
        {
            stateMachine.ChangeState(shieldSoldier.idleState);

            /*if (comboCounter >= 3 || !shieldSoldier.IsPlayerInAttackRange())
            {
                shieldSoldier.nextTimeReadyToComboAttack = Time.time + shieldSoldier.ComboAttackCD;
                stateMachine.ChangeState(shieldSoldier.battleState);
            }
            else
            {
                stateMachine.ChangeState(shieldSoldier.comboAttackState);
            }*/
        }
    }
}
