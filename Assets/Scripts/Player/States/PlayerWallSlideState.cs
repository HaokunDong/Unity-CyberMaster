using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlideState : PlayerState
{
    public PlayerWallSlideState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.Flip();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        
        base.Update();

        if (xInput != 0 && player.facingDir == xInput)
        {
            stateMachine.ChangeState(player.idleState);
        }

        if (Input.GetKey(KeyCode.K))
        {
            stateMachine.ChangeState(player.wallJumpState);
            return;
        }

        if (yInput < 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y * 0.5f);
        }

        if (player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
