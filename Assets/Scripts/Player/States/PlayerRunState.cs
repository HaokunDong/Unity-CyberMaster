using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerGroundedState
{
    public PlayerRunState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
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
        player.SetVelocity(xInput * player.moveSpeed * 3, rb.velocity.y);

        if (xInput == 0)
        {
            stateMachine.ChangeState(player.idleState);
        }

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            stateMachine.ChangeState(player.moveState);
        }
    }
}
