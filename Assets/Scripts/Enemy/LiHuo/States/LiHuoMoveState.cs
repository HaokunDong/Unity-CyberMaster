using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoMoveState : LiHuoGroundState
{
    /*public LiHuoMoveState(EnemyStateMachine _stateMachine, Enemy _enemy, EnemyBehaviourTree _enemyBehaviour, string _animBoolName, Enemy_Boss_LiHuo _liHuo, LihuoBehaviourTree _liHuoBehaviour)
        : base(_stateMachine, _enemy, _enemyBehaviour, _animBoolName, _liHuo, _liHuoBehaviour)
    {
    }*/
    public LiHuoMoveState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo)
        : base(_stateMachine, _enemy, _animBoolName, _liHuo)
    {
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

        liHuo.SetVelocity(liHuo.facingDir * liHuo.moveSpeed, rb.velocity.y);

        if (enemy.RelativeDistance() < enemy.attackDistance)
        {
            stateMachine.ChangeState(liHuo.battleState);
        }

/*        if (liHuo.facingDir != liHuo.RelativePosition())
        {
            stateMachine.ChangeState(liHuo.idleState);
        }*/
    }
}
