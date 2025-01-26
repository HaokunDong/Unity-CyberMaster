using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Boss_LiHuo : Enemy
{
    public Player player;

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

    }

    public override void HitTarget()
    {
        base.HitTarget();

        SetMovement(player.attackForce[player.attackCount] * player.facingDir, rb.velocity.y);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.DrawWireSphere(attackCheck[attackCount].position, attackCheckRadius[attackCount]);
    }
}
