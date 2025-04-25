using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTrees;

public class EnemyState
{
    protected EnemyStateMachine stateMachine;
    protected Enemy enemy;

    protected Rigidbody2D rb;

    private string animBoolName;

    protected float stateTimer;
    protected bool triggerCalled;

    protected int bounceAttackCounter;

    protected BTNode behaviourTree;

    public EnemyState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName)
    {
        stateMachine = _stateMachine;
        enemy = _enemy;
        animBoolName = _animBoolName;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;

    }

    public virtual void Enter()
    {
        enemy.animator.SetBool(animBoolName, true);
        rb = enemy.rb;
        triggerCalled = false;
    }

    public virtual void Exit()
    {
        enemy.animator.SetBool(animBoolName, false);
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
    public virtual void OnHit(Entity from)
    {

    }
}
