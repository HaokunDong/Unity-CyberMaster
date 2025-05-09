using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoPrimaryAttackState : LiHuoComboAttackState
{
    protected int comboCounter;
    protected float lastTimeAttacked;
    protected float comboWindow = 0.3f;

    public LiHuoPrimaryAttackState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo)
        : base(_stateMachine, _enemy, _animBoolName, _liHuo)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (comboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow)
        {
            comboCounter = 0;
        }

        liHuo.attackCount = comboCounter;
        liHuo.animator.SetInteger("ComboCounter", comboCounter);

        stateTimer = 0.1f;

        liHuo.SetVelocity(liHuo.attackMovement[comboCounter].x * liHuo.facingDir, liHuo.attackMovement[comboCounter].y);

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
            stateMachine.ChangeState(liHuo.comboAttackState);
            /*if (comboCounter == 2)
            {
                stateMachine.ChangeState(liHuo.cdState);
            }
            else
            {
                stateMachine.ChangeState(liHuo.comboAttackState);
            }*/
        }
    }
}
