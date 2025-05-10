using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTrees;

public class Enemy_Boss_LiHuo : Enemy
{
    public Player player;

    public float stabDistance;

    [Header("LiHuoSkills Info")]
    private float nextTimeReadyToMove;
    [SerializeField] public float moveCD;
    private float nextTimeReadyToLeapAttack;
    [SerializeField] public float leapAttackCD;
    private float nextTimeReadyToStabAttack;
    [SerializeField] public float stabAttackCD;

    //public bool bossFightBegun;

    #region States

    public LiHuoIdleState idleState { get; private set; }
    public LiHuoMoveState moveState { get; private set; }
    public LiHuoBattleState battleState { get; private set; }
    public LiHuoCDState cdState { get; private set; }
    public LiHuoComboAttackState comboAttackState { get; private set; }
    public LiHuoPrimaryAttackState primaryAttackState { get; private set; }
    public LiHuoLeapAttackState leapAttackState { get; private set; }
    public LiHuoStabAttackState stabAttackState { get; private set; }
    public LiHuoBounceAttackState bounceAttackState { get; private set; }
    public LiHuoDeadState deadState { get; private set; }
    public LiHuoBeExecutedState beExecutedState { get; private set; }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        idleState = new LiHuoIdleState(stateMachine, this, "Idle", this);
        moveState = new LiHuoMoveState(stateMachine, this, "Move", this);
        battleState = new LiHuoBattleState(stateMachine, this, "Idle", this);
        cdState = new LiHuoCDState(stateMachine, this, "Idle", this);
        comboAttackState = new LiHuoComboAttackState(stateMachine, this, "Idle", this);
        primaryAttackState = new LiHuoPrimaryAttackState(stateMachine, this, "Attack", this);
        leapAttackState = new LiHuoLeapAttackState(stateMachine, this, "LeapAttack", this);
        stabAttackState = new LiHuoStabAttackState(stateMachine, this, "StabAttack", this);
        bounceAttackState = new LiHuoBounceAttackState(stateMachine, this, "BounceAttack", this);
        deadState = new LiHuoDeadState(stateMachine, this, "Idle", this);
        beExecutedState = new LiHuoBeExecutedState(stateMachine, this, "BeExecuted", this);

    }


    protected override void Start()
    {
        base.Start();

        moveSpeed = moveSpeed * 1.5f;

        Flip();

        stateMachine.Initialize(idleState);

    }

    protected override void Update()
    {
        base.Update();
    }

    public override void OnHitFromTarget(Entity from)
    {
        base.OnHitFromTarget(from);
        SetMovement(player.attackForce[player.attackCount] * player.facingDir, rb.velocity.y);

    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.DrawWireSphere(attackCheck[attackCount].position, attackCheckRadius[attackCount]);
    }

    #region DistanceJudge
    public bool IsPlayerInViewRange()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < 10)
        {
            return true;
        }
        return false;
    }

    public bool IsEnterAttackState()//Enter Attack State
    {
        if (RelativeDistance() < attackDistance - 1)
        {
            return true;
        }
        return false;
    }
    public bool IsPlayerInAttackRange()//After enter attack state
    {
        if (RelativeDistance() < attackDistance)
        {
            return true;
        }
        return false;
    }

    public bool IsPlayerInStabRange()
    {
        if (RelativeDistance() < stabDistance)
        {
            return true;
        }
        return false;
    }

    public bool IsPlayerFar()
    {
        if (RelativeDistance() > 5.0f)
        {
            return true;
        }
        return false;
    }

    public bool IsPlayerClose()
    {
        if (RelativeDistance() <= 5.0f)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region CanUseSkill
    public bool CanMove()
    {
        if (Time.time >= nextTimeReadyToMove)
        {
            nextTimeReadyToMove = Time.time + moveCD;
            return true;
        }
        return false;
    }

    public bool CanComboAttack()
    {
        if (Time.time >= nextTimeReadyToComboAttack)
        {
            nextTimeReadyToComboAttack = Time.time + ComboAttackCD;
            return true;
        }
        return false;
    }

    public bool CanLeapAttack()
    {
        if (Time.time >= nextTimeReadyToLeapAttack)
        {
            nextTimeReadyToLeapAttack = Time.time + leapAttackCD;
            return true;
        }
        return false;
    }

    public bool CanStabAttack()
    {
        if (Time.time >= nextTimeReadyToStabAttack)
        {
            nextTimeReadyToStabAttack = Time.time + stabAttackCD;
            return true;
        }
        return false;
    }

    #endregion
}
