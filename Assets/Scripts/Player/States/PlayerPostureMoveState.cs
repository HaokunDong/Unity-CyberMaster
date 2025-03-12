using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPostureMoveState : PlayerPostureState
{
    public PlayerPostureMoveState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
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

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.postureState);
        }
    }
}
