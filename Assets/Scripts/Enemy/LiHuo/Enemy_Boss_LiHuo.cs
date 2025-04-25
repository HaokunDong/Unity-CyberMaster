using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTrees;

public class Enemy_Boss_LiHuo : Enemy
{
    public Player player;
    //private BTNode behaviourTree;

    //public bool bossFightBegun;

    #region States

    public LiHuoIdleState idleState { get; private set; }
    public LiHuoMoveState moveState { get; private set; }
    public LiHuoBattleState battleState { get; private set; }
    public LiHuoPrimaryAttackState primaryAttackState { get; private set; }
    public LiHuoBounceAttackState bounceAttackState { get; private set; }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        idleState = new LiHuoIdleState(stateMachine, this, "Idle", this);
        moveState = new LiHuoMoveState(stateMachine, this, "Move", this);
        battleState = new LiHuoBattleState(stateMachine, this, "Idle", this);
        primaryAttackState = new LiHuoPrimaryAttackState(stateMachine, this, "Attack", this);
        bounceAttackState = new LiHuoBounceAttackState(stateMachine, this, "BounceAttack", this);

        //BuildLihuoBehaviourTree();
    }


    protected override void Start()
    {
        base.Start();

        this.moveSpeed = base.moveSpeed * 1.5f;

        Flip();

        stateMachine.Initialize(idleState);

    }

    protected override void Update()
    {
        base.Update();

        //behaviourTree.Execute();
    }

    public override void HitTarget(Entity from)
    {
        base.HitTarget(from);
        stateMachine.currentState.OnHit(from);
        SetMovement(player.attackForce[player.attackCount] * player.facingDir, rb.velocity.y);

    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.DrawWireSphere(attackCheck[attackCount].position, attackCheckRadius[attackCount]);
    }

    /*public void BuildLihuoBehaviourTree()
    {
        behaviourTree = new Selector(
            new Sequencer(
                new ConditionNode(() => IsPlayerInAttackRange()),
                new ActionNode(() => stateMachine.ChangeState(primaryAttackState))
                ),

            new Sequencer(
                new ConditionNode(() => IsPlayerInViewRange()),
                new ActionNode(() => stateMachine.ChangeState(moveState))
                ),

            new ActionNode(() => stateMachine.ChangeState(idleState))
            );
    }*/

    public bool IsPlayerInAttackRange()
    {
        if (RelativeDistance() < attackDistance)
        {
            if (CanAttack())
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPlayerInViewRange()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < 10)
        {
            return true;
        }
        return false;
    }
}
