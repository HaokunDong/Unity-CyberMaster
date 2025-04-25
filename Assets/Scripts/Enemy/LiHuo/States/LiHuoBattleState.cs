using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTrees;

public class LiHuoBattleState : EnemyState
{
    private Enemy_Boss_LiHuo liHuo;
    public LiHuoBattleState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo)
        : base(_stateMachine, _enemy, _animBoolName)
    {
        liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();

        BuildLihuoBehaviourTree();
    }
    
    public override void Exit()
    {
        base.Exit();

    }

    public override void Update()
    {
        base.Update();

        Debug.Log("battle");

        behaviourTree.Execute();

        /*if (enemy.RelativeDistance() < enemy.attackDistance)
        {
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
        }*/
    }

    public void BuildLihuoBehaviourTree()
    {
        behaviourTree = new Selector(
            new Sequencer(
                new ConditionNode(() => IsPlayerInAttackRange()),
                new ActionNode(() => stateMachine.ChangeState(liHuo.primaryAttackState))
                ),

            new Sequencer(
                new ConditionNode(() => liHuo.facingDir != liHuo.RelativePosition()),
                new ActionNode(() => liHuo.FlipController(liHuo.RelativePosition()))
                ),

            new Sequencer(
                new ConditionNode(() => liHuo.IsPlayerInViewRange()),
                new ActionNode(() => stateMachine.ChangeState(liHuo.moveState))
                ),

            new ActionNode(() => stateMachine.ChangeState(liHuo.idleState))
            );
    }

    public bool IsPlayerInAttackRange()
    {
        if (enemy.RelativeDistance() < enemy.attackDistance)
        {
            if (enemy.CanAttack())
            {
                return true;
            }
        }
        return false;
    }

}

