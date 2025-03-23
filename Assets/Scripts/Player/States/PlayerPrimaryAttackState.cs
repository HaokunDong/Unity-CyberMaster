using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttackState : PlayerState
{

    protected int comboCounter;
    protected float lastTimeAttacked;
    protected float comboWindow = 0.3f;

    public PlayerPrimaryAttackState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();

        xInput = 0;

        if (comboCounter > 4 || Time.time >= lastTimeAttacked + comboWindow)
        {
            comboCounter = 0;
        }

        player.attackCount = comboCounter;
        player.animator.SetInteger("ComboCounter", comboCounter);

        player.SetVelocity(player.attackMovement[comboCounter].x * player.facingDir, player.attackMovement[comboCounter].y);

        stateTimer = 0.1f;

    }

    public override void Exit()
    {
        base.Exit();
        if (xInput != 0 && player.facingDir != xInput)
        {
            player.Flip();
        }

        comboCounter++;
        lastTimeAttacked = Time.time;

        player.StartCoroutine("BusyFor", 0.15f);
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            player.SetZeroVelocity();
        }

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

}
