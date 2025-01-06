using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    [Header("Attack Details")]
    public Vector2[] attackMovement;//���ڿ���ÿ�ι�����ǰ�����ϵ�λ��
    public float chargeAttackMovement;

    public bool isBusy { get; private set; }

    [Header("Move Info")]
    public float moveSpeed = 3f;
    public float jumpForce = 12f;
    [Header("Dodge Info")]
    public float dodgeSpeed = 10f;
    public float dodgeDir { get; private set; }

    public float velocityY { get; private set; }

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

    public PlayerPrimaryAttackState primaryAttack { get; private set; }
    public PlayerChargeAttackState chargeAttack { get; private set; }
    #endregion


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

        primaryAttack = new PlayerPrimaryAttackState(stateMachine, this, "Attack");
        chargeAttack = new PlayerChargeAttackState(stateMachine, this, "ChargeAttack");
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
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

    //ʹ��Э�̣����������ǽ�isBusy���false�Ĺ��̽���һ���ӳ�
    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;

        yield return new WaitForSeconds(_seconds);

        isBusy = false;
    }

    public void SetDodgeDirection()
    {
        dodgeDir = Input.GetAxisRaw("Horizontal");

        if (dodgeDir == 0)
        {
            dodgeDir = facingDir;
        }
    }

    //����AnimationTriggerʱ���Ե��õ�PlayerState�е�AnimationFinishTrigger����
    //���Դ�unity�Ķ�������֡�ϵ��ô˺���
    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    //֡�¼����ڳ��ܹ�����λ��
    public void ChargeAttackMove() => stateMachine.currentState.AnimationEventTrigger();

}
