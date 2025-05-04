using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandState : PlayerState
{
    public PlayerLandState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
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

        float xInput = Input.GetAxisRaw("Horizontal");
        float xVelocity = xInput * player.moveSpeed;
        player.SetVelocity(xVelocity, rb.velocity.y); // 保持落地时流畅移动

        if (triggerCalled)
        {
            if (xInput != 0)
                stateMachine.ChangeState(player.moveState);
            else
                stateMachine.ChangeState(player.idleState);
        }
}

}
