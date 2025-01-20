using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChargeAttackState : PlayerState
{
    public PlayerChargeAttackState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
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

        if (stateTimer < 0)
        {
            player.SetZeroVelocity();
        }

        if (eventsTriggerCalled)
        {
            stateTimer = 0.2f;
            player.SetVelocity(player.facingDir * player.chargeAttackMovement, rb.velocity.y);

            eventsTriggerCalled = false;
        }

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
