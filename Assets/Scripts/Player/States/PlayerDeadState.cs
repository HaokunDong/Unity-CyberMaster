using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.SetMovement(-player.facingDir * 20, rb.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
