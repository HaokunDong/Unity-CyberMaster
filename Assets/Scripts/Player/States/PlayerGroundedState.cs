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
            if (IsStunned())
            {
                player.attackLimitation = 0;
                stateMachine.ChangeState(player.beStunnedState);
            }
            else if (player.canExecution && player.RaycastDetectEnemy())
            {
                player.canExecution = false;
                stateMachine.ChangeState(player.executionState);
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

    public bool IsStunned()
    {
        Collider2D []colliders = Physics2D.OverlapCircleAll
            (player.attackCheck[player.attackCount].position, player.attackCheckRadius[player.attackCount]);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null && player.attackLimitation >= 100)
            {
                return true;
            }
        }
        return false;
    }
}
