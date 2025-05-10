using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExecutionState : PlayerState
{
    protected Vector2 originalPosition;
    public PlayerExecutionState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        originalPosition = player.transform.position;

        player.ChangeLayer(player.gameObject, "Dodge");
        rb.position = new Vector2(EnemyManager.Ins.liHuo.transform.position.x, rb.position.y);
    }

    public override void Exit()
    {
        base.Exit();

        rb.position = originalPosition;

        player.ChangeLayer(player.gameObject, "Player");
    }

    public override void Update()
    {
        base.Update();

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
