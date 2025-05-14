using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoPrimaryAttackState : EnemyState
{
    protected Enemy_Boss_LiHuo liHuo;

    protected int comboCounter;
    protected float lastTimeAttacked;
    protected float comboWindow = 0.3f;

    public LiHuoPrimaryAttackState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo)
        : base(_stateMachine, _enemy, _animBoolName)
    {
        liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();

        if (comboCounter > 5 || Time.time >= lastTimeAttacked + comboWindow)
        {
            comboCounter = 0;
        }

        liHuo.attackCount = comboCounter;
        liHuo.animator.SetInteger("ComboCounter", comboCounter);

        stateTimer = 0.1f;

        liHuo.SetVelocity(liHuo.attackMoveSpeed[comboCounter].x * liHuo.facingDir, liHuo.attackMoveSpeed[comboCounter].y);

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
            liHuo.SetZeroVelocity();
        }

        if (triggerCalled)
        {
            if (comboCounter >= 5 || !liHuo.IsPlayerInAttackRange())
            {
                liHuo.nextTimeReadyToComboAttack = Time.time + liHuo.ComboAttackCD;
                stateMachine.ChangeState(liHuo.battleState);
            }
            else
            {
                stateMachine.ChangeState(liHuo.comboAttackState);
            }
        }
    }
}
