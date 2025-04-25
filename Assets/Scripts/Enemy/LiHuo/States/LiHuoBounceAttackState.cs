using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoBounceAttackState : EnemyState
{
    private Enemy_Boss_LiHuo liHuo;
    public LiHuoBounceAttackState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo)
        : base(_stateMachine, _enemy, _animBoolName)
    {
        liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();

        if (bounceAttackCounter > 1)
        {
            bounceAttackCounter = 0;
        }
        liHuo.animator.SetInteger("BounceAttackCounter", bounceAttackCounter);
    }

    public override void Exit()
    {
        base.Exit();

        bounceAttackCounter++;
    }

    public override void Update()
    {
        base.Update();

        /*if (triggerCalled)
        {
            stateMachine.ChangeState(liHuo.idleState);
        }*/
    }

}
