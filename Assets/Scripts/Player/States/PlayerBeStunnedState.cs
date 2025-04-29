using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBeStunnedState : PlayerState
{
    public PlayerBeStunnedState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();

        //EnemyManager.Ins.liHuo.HitTarget(player);
        
        player.attackLimitation = 0;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        player.SetZeroVelocity();

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
