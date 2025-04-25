using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public Enemy_Boss_LiHuo liHuo;

    [Header("Attack Details")]
    public int attackCount;
    public Vector2[] attackMovement;
    public float[] attackForce;
    public float chargeAttackMovement;
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

    public PlayerPrimaryAttackState primaryAttackState { get; private set; }
    public PlayerChargeAttackState chargeAttackState { get; private set; }
    #endregion

    //public Animator animator { get; private set; }

    protected override void Awake()
    {
        base.Awake();
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

        primaryAttackState = new PlayerPrimaryAttackState(stateMachine, this, "Attack");
        chargeAttackState = new PlayerChargeAttackState(stateMachine, this, "ChargeAttack");
    }

    protected override void Start()
    {
        base.Start();
        //animator = GetComponentInChildren<Animator>();

        //liHuo = EnemyManager.Ins.liHuo;

        stateMachine.Initialize(idleState);
        BarInfo lifeInfo = new BarInfo()
        {
            title = "刃势",
            segments = new List<SegmentInfo>(){
                new SegmentInfo() { weight = 1, color = Color.red, text = "竭刃" },
                new SegmentInfo() { weight = 1, color = Color.green , text = "盈刃" },
            }
        };
        this.lifeBar.SetInfo(lifeInfo);
        this.RefreshInfoState();
    }
    public void EnterRoom()
    {
        GameManager.Ins.Send(GameEvent.OnPlayerEnter, this);
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();

        SetDodgeDirection();
    }

    public override void HitTarget(Entity from)
    {
        base.HitTarget(from);

        if (isInvincible) return;

        SetMovement(liHuo.attackForce[liHuo.attackCount] * liHuo.facingDir, rb.velocity.y);
        stateMachine.currentState.OnHit(from);
        RefreshInfoState();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.DrawWireSphere(attackCheck[attackCount].position, attackCheckRadius[attackCount]);
    }

    public IEnumerator BusyFor(float _seconds)//used to control the attack interval
    {
        isBusy = true;

        yield return new WaitForSeconds(_seconds);

        isBusy = false;
    }
    public IEnumerator Invincibility()
    {
        isInvincible = true;
        yield return new WaitForSeconds(0.2f);
        isInvincible = false;
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
        this.lifeBar.t = info.life / 100;
    }

}
