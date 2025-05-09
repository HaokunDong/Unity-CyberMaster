using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTrees;

public class LiHuoMoveState : EnemyState
{
    protected Enemy_Boss_LiHuo liHuo;
    public LiHuoMoveState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo)
        : base(_stateMachine, _enemy, _animBoolName)
    {
        liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = 2.0f;

        BuildLihuoBehaviourTree();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        behaviourTree.Execute();

        liHuo.SetVelocity(liHuo.facingDir * liHuo.moveSpeed, rb.velocity.y);
    }

    public void BuildLihuoBehaviourTree()
    {
        behaviourTree = new Selector(
            new Sequencer(
                new ConditionNode(() => liHuo.facingDir != liHuo.RelativePosition()),
                new ActionNode(() => stateMachine.ChangeState(liHuo.cdState))
                ),

            new Sequencer(
                new ConditionNode(() => liHuo.IsPlayerInAttackRange()),
                new ActionNode(() => stateMachine.ChangeState(liHuo.battleState))
                ),

            new Sequencer(
                new ConditionNode(() => stateTimer <= 0),
                new ActionNode(() => stateMachine.ChangeState(liHuo.battleState))
                )
            );
    }
}
