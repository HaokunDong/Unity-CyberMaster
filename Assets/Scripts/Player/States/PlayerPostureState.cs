using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPostureState : PlayerState
{
    public PlayerPostureState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();
        postureTimer = 0;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (!Input.GetKey(KeyCode.Space))
        {
            stateMachine.ChangeState(player.idleState);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll
            (player.attackCheck[player.attackCount].position, player.attackCheckRadius[player.attackCount]);

            foreach (var hit in colliders)
            {
                if (hit.GetComponent<Enemy>() != null)
                {
                    if (hit.GetComponent<Enemy>().canBeBouncedAttack)
                    {
                        stateMachine.ChangeState(player.bounceAttackState);
                    }
                }
            }
        }


    }
}
