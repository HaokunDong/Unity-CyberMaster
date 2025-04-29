using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    public PlayerGroundedState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
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


        if (Input.GetKeyDown(KeyCode.L))
        {
            stateMachine.ChangeState(player.dodgeState);
        }

        if (Input.GetKey(KeyCode.J))
        {
            if (player.attackLimitation >= 10)
            {
                stateMachine.ChangeState(player.beStunnedState);
            }
            else
            {
                stateMachine.ChangeState(player.primaryAttackState);
            }
        }

        if (Input.GetKey(KeyCode.U))
        {
            stateMachine.ChangeState(player.chargeAttackState);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            stateMachine.ChangeState(player.postureState);
        }

        if (Input.GetKeyDown(KeyCode.K) && player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.jumpState);
        }

        if (!player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.airState);
        }
    }
}
