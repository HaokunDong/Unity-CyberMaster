using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttackState : PlayerState
{
    private int comboCounter;

    private float lastTimeAttacked;
    //comboWindow用于定义玩家在进行连续攻击（连击）时，两次攻击动作之间允许的最大间隔时间。
    //如果玩家在这段时间内没有执行下一次攻击，那么连击计数器（comboCounter）会重置.
    private float comboWindow = 0.3f;


    public PlayerPrimaryAttackState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (comboCounter > 4 || Time.time >= lastTimeAttacked + comboWindow)//Time.time(现在的时间) 若超过了: 最后攻击的时间 + 时间间隔(秒)
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
