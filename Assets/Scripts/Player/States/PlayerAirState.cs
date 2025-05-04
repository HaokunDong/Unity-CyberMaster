using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }
    public override void Update()
    {
        base.Update();

        float xInput = Input.GetAxisRaw("Horizontal"); // 建议确认这一行在 PlayerState 里有定义
        float targetSpeed = xInput * player.runSpeed;

        player.SetVelocity(targetSpeed, rb.velocity.y); // 立即响应方向

        if (player.IsWallDetected())
        {
            stateMachine.ChangeState(player.wallSlideState);
        }

        if (player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.landState);
        }
    }
    // public override void Update()
    // {
    //     base.Update();

    //     float targetSpeed = player.moveSpeed * xInput;

    //     // 只有当 x 方向输入发生变化时，才调整速度
    //     if (Mathf.Abs(targetSpeed - rb.velocity.x) > 0.1f)
    //     {
    //         float smoothedSpeed = Mathf.MoveTowards(rb.velocity.x, targetSpeed, Time.deltaTime * player.moveSpeed * 2f);
    //         player.SetVelocity(smoothedSpeed, rb.velocity.y);
    //     }

    //     if (player.IsWallDetected())
    //     {
    //         stateMachine.ChangeState(player.wallSlideState);
    //     }

    //     if (player.IsGroundDetected())
    //     {
    //         stateMachine.ChangeState(player.landState);
    //     }
    // }

}
