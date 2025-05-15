using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTrees;

public class LiHuoBattleState : EnemyState
{
    protected Enemy_Boss_LiHuo liHuo;
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

        behaviourTree.Execute();
    }

    public void BuildLihuoBehaviourTree()
    {
        behaviourTree = new Selector(
            new Sequencer(
                new ConditionNode(() => liHuo.facingDir != liHuo.RelativePosition()),
                new ActionNode(() => stateMachine.ChangeState(liHuo.cdState))
                //new ActionNode(() => liHuo.StartCoroutine(liHuo.DelayFlip()))
                ),

            new Sequencer(
                new ConditionNode(() => liHuo.IsPlayerFar()),
                new RandomSelector(
                    new Sequencer(
                        new ConditionNode(() => liHuo.CanLeapAttack()),
                        new ActionNode(() => stateMachine.ChangeState(liHuo.leapAttackState))
                        ),
                    new Sequencer(
                        new ConditionNode(() => liHuo.CanMove()),
                        new ActionNode(() => stateMachine.ChangeState(liHuo.moveState))
                        )
                    )
                ),

            new Sequencer(
                new ConditionNode(() => liHuo.IsPlayerClose()),
                new Selector(
                    new Sequencer(
                        new ConditionNode(() => liHuo.IsEnterAttackState() && liHuo.CanComboAttack()),
                        new ActionNode(() => stateMachine.ChangeState(liHuo.primaryAttackState))
                        ),

                    new Sequencer(
                        new ConditionNode(() => liHuo.IsPlayerInStabRange()),
                        new RandomSelector(
                            new Sequencer(
                                new ConditionNode(() => liHuo.CanStabAttack()),
                                new ActionNode(() => stateMachine.ChangeState(liHuo.stabAttackState))
                                ),
                            new Sequencer(
                                new ConditionNode(() => liHuo.CanMove()),
                                new ActionNode(() => stateMachine.ChangeState(liHuo.moveState))
                                )
                            )
                        ),

                    new ActionNode(() => stateMachine.ChangeState(liHuo.cdState))
                    )
                ),

            new ActionNode(() => stateMachine.ChangeState(liHuo.idleState))
            );
    }

    
}

