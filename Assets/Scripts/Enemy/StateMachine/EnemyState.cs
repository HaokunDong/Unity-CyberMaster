using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState
{
    protected EnemyStateMachine stateMachine;
    protected Enemy enemy;
    protected Rigidbody2D rb;

    private string animBoolName;

    protected float stateTimer;
    protected bool triggerCalled;
    protected bool PreprocessTriggerCalled;

    protected int comboCounter;
    protected float lastTimeAttacked;
    protected float comboWindow = 0.3f;

    protected int bounceAttackCounter;

    public EnemyState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName)
    {
        this.stateMachine = _stateMachine;
        this.enemy = _enemy;
        this.animBoolName = _animBoolName;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
    }

    public virtual void Enter()
    {
        enemy.animator.SetBool(animBoolName, true);
        rb = enemy.rb;
        PreprocessTriggerCalled = false;
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

    public virtual void AnimationPreprocessTrigger()
    {
        PreprocessTriggerCalled = true;
    }
}
