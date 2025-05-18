using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : Entity
{
    public Enemy_Boss_LiHuo liHuo;

    [Header("Attack Details")]
    public int attackCount;
    public int attackLimitation = 0;//Limit the attack combo count.
    public Vector2[] attackMovement;
    public float[] attackForce;
    [Header("Skill Info")]
    public float chargeAttackMovement;
    public Transform chargeAttackCheck;
    public float chargeAttackRadius;
    public bool isBusy { get; private set; }

    [Header("Move Info")]
    public float moveSpeed = 3f;
    public float runSpeed = 9f;
    public float jumpForce = 12f;
    [Header("Dodge Info")]
    public float dodgeSpeed = 10f;
    public float dodgeDir { get; private set; }
    public bool isInvincible = false;

    public float velocityY { get; private set; }
    [Header("Bar Com")]
    public Transform barFolder;
    public BarCom lifeBar;
    public PlayerInfo info = new PlayerInfo();

    [Header("Execution Info")]
    public UnityEvent OnExecution;
    public bool canExecution = false;
    public float executionRange;

    [Header("Dead Info")]
    public UnityEvent PlayerDead;
    #region States
    public PlayerStateMachine stateMachine { get; private set; }

    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerRunState runState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerLandState landState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerWallSlideState wallSlideState { get; private set; }
    public PlayerWallJumpState wallJumpState { get; private set; }
    public PlayerDodgeState dodgeState { get; private set; }
    public PlayerPostureState postureState { get; private set; }
    public PlayerPostureMoveState postureMoveState { get; private set; }
    public PlayerBounceAttackState bounceAttackState { get; private set; }
    public PlayerBeAttackedState beAttackedState { get; private set; }
    public PlayerBeStunnedState beStunnedState { get; private set; }
    public PlayerExecutionState executionState { get; private set; }
    public PlayerPrimaryAttackState primaryAttackState { get; private set; }
    public PlayerChargeAttackState chargeAttackState { get; private set; }

    public PlayerDeadState deadState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        //liHuo = EnemyManager.Ins.liHuo;
        stateMachine = new PlayerStateMachine();

        idleState = new PlayerIdleState(stateMachine, this, "Idle");
        moveState = new PlayerMoveState(stateMachine, this, "Move");
        runState = new PlayerRunState(stateMachine, this, "Run");
        jumpState = new PlayerJumpState(stateMachine, this, "Jump");
        landState = new PlayerLandState(stateMachine, this, "Landing");
        airState = new PlayerAirState(stateMachine, this, "Jump");
        wallSlideState = new PlayerWallSlideState(stateMachine, this, "WallSlide");
        wallJumpState = new PlayerWallJumpState(stateMachine, this, "Jump");
        dodgeState = new PlayerDodgeState(stateMachine, this, "Dodge");
        postureState = new PlayerPostureState(stateMachine, this, "Posture");
        postureMoveState = new PlayerPostureMoveState(stateMachine, this, "PostureMove");
        bounceAttackState = new PlayerBounceAttackState(stateMachine, this, "BounceAttack");
        beAttackedState = new PlayerBeAttackedState(stateMachine, this, "BeAttacked");
        beStunnedState = new PlayerBeStunnedState(stateMachine, this, "BeStunned");
        executionState = new PlayerExecutionState(stateMachine, this, "Execution");

        primaryAttackState = new PlayerPrimaryAttackState(stateMachine, this, "Attack");
        chargeAttackState = new PlayerChargeAttackState(stateMachine, this, "ChargeAttack");

        deadState = new PlayerDeadState(stateMachine, this, "Dead");
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);

        this.RefreshInfoState();
        lifeBar.On(BarComEvent.MAX_ARRIVE, PlayerWin);
        lifeBar.On(BarComEvent.MIN_ARRIVE, PlayerLose);
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();

        SetDodgeDirection();
    }

    public override void HitTarget()
    {
        base.HitTarget();

        attackLimitation++;

    }

    public override void OnHitFromTarget(Entity from)
    {
        base.OnHitFromTarget(from);

        if (isInvincible) return;

        SetMovement(liHuo.attackForce[liHuo.attackCount] * liHuo.facingDir, rb.velocity.y);
        stateMachine.currentState.OnHit(from);
        RefreshInfoState();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        //Gizmos.DrawWireSphere(attackCheck[attackCount].position, attackCheckRadius[attackCount]);

        Gizmos.DrawWireSphere(chargeAttackCheck.position, chargeAttackRadius);
    }

    public bool RaycastDetectEnemy()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.right, executionRange);
        foreach (var hit in hits)
        {
            if (hit.collider.GetComponent<Enemy>() != null)
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerator BusyFor(float _seconds)//used to control the attack interval
    {
        isBusy = true;

        yield return new WaitForSeconds(_seconds);

        isBusy = false;
    }
    public IEnumerator Invincibility()//Invincible in dodge state 
    {
        isInvincible = true;
        yield return new WaitForSeconds(0.2f);
        isInvincible = false;
    }

    public void PlayerWin()
    {
        Debug.Log("Player Win!");
        canExecution = true;
        OnExecution.Invoke();
    }
    public void PlayerLose()
    {
        Debug.Log("Player Lose!");
        PlayerDead.Invoke();
    }

    public void ChangeLayer(GameObject obj, string layerName)
    {
        obj.layer = LayerMask.NameToLayer(layerName);
        foreach (Transform child in obj.transform)
        {
            ChangeLayer(child.gameObject, layerName);
        }
    }

    public virtual int RelativePosition()//与Enemy的相对位置
    {
        if (this.transform.position.x - liHuo.transform.position.x < 0)
        {
            return 1;
        }
        else if (this.transform.position.x - liHuo.transform.position.x > 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public void SetDodgeDirection()
    {
        dodgeDir = facingDir;
        /*dodgeDir = Input.GetAxisRaw("Horizontal");

        if (dodgeDir == 0)
        {
            dodgeDir = facingDir;
        }*/
    }

    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    public void ChargeAttackMove() => stateMachine.currentState.AnimationEventTrigger();
    public void RefreshInfoState()
    {
        this.lifeBar.t = (info.life - 50) / 50;
    }
    
    public void TriggerExecutionRetreat()
    {
        float retreatDistance = 5f;
        float retreatDuration = 0.4f;
        StartCoroutine(SlideBack(retreatDistance, retreatDuration));
    }

    private IEnumerator SlideBack(float distance, float duration)
    {
        Vector2 start = rb.position;
        Vector2 end = start + new Vector2(-facingDir * distance, 0); // 角色朝背后移动

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rb.position = Vector2.Lerp(start, end, t);
            yield return null;
        }
    }

}
