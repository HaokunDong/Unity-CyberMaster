using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerState
{
    public PlayerWallJumpState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = 0.4f;

        player.SetVelocity(player.moveSpeed * player.facingDir * 3, player.jumpForce * 0.8f);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(player.airState);
        }
        if (player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
        if (player.IsWallDetected())
        {
            stateMachine.ChangeState(player.wallSlideState);
        }
    }
}
