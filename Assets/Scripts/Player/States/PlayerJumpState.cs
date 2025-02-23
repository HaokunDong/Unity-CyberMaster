using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private float jumpStartTime;
    private float minJumpDuration = 0.1f; // 最少跳跃持续时间

    public PlayerJumpState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();
        jumpStartTime = Time.time; // 记录跳跃时间

        // 仅在有移动速度时才施加水平速度，防止原地跳跃前冲
        float xVelocity = (rb.velocity.x != 0) ? player.runSpeed * player.facingDir : rb.velocity.x;
        player.SetVelocity(xVelocity, player.jumpForce);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // 确保跳跃状态至少持续 0.1 秒
        if (Time.time > jumpStartTime + minJumpDuration && !player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.airState);
        }
    }
}
