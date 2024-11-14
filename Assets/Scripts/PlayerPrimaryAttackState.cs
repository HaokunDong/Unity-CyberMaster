using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttackState : PlayerState
{
    private int comboCounter;

    private float lastTimeAttacked;
    private float comboWindow = 2;

    public PlayerPrimaryAttackState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if(comboCounter > 2 || Time.time >= lastTimeAttacked + 0.5f)//Time.time(现在的时间) 若超过了: 最后攻击的时间 + 攻击的次数(秒)
        {
            comboCounter = 0;
        }

        player.animator.SetInteger("ComboCounter", comboCounter);

        if(xInput != 0 && player.facingDir != xInput)
        {
            player.Flip();
        }
        player.SetVelocity(player.attackMovement[comboCounter].x * player.facingDir, player.attackMovement[comboCounter].y);

        stateTimer = 0.1f;
    }

    public override void Exit()
    {
        base.Exit();

        player.StartCoroutine("BusyFor", 0.15f);

        comboCounter++;

        lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            player.ZeroVelocity();
        }

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
