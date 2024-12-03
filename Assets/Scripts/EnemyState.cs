using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState
{
    protected EnemyStateMachine stateMachine;
    protected Enemy enemy;

    private string animBoolName;

    protected float stateTimer;
    protected bool triggerCalled;

    public EnemyState(EnemyStateMachine stateMachine, Enemy enemy, string animBoolName)
    {
        this.stateMachine = stateMachine;
        this.enemy = enemy;
        this.animBoolName = animBoolName;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
    }

    public virtual void Enter()
    {
        triggerCalled = false;

        enemy.animator.SetBool(animBoolName, true);
    }

    public virtual void Exit()
    {
        enemy.animator.SetBool(animBoolName, false);

    }
}
