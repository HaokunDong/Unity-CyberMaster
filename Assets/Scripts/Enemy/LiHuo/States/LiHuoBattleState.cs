using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoBattleState : EnemyState
{
    private Enemy_Boss_LiHuo liHuo;
    public LiHuoBattleState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) : base(_stateMachine, _enemy, _animBoolName)
    {
        this.liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();

        //Debug.Log(liHuo.attackCount);
    }
    
    public override void Exit()
    {
        base.Exit();

    }

    public override void Update()
    {
        base.Update();

        if (enemy.RelativeDistance() < enemy.attackDistance)
        {
            //Debug.Log(enemy.RelativeDistance());
            if (enemy.CanAttack())
            {
                stateMachine.ChangeState(liHuo.primaryAttackState);
            }
        }
        else
        {
            stateMachine.ChangeState(liHuo.idleState);
        }

        if (liHuo.facingDir != liHuo.RelativePosition())
        {
            stateMachine.ChangeState(liHuo.idleState);
        }
    }
}
