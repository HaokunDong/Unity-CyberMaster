using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoAppearState : EnemyState
{
    protected Enemy_Boss_LiHuo LiHuo;
    public LiHuoAppearState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) 
        : base(_stateMachine, _enemy, _animBoolName)
    {
        LiHuo = _liHuo;
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

        LiHuo.SetZeroVelocity();

        if (triggerCalled)
        {
            stateMachine.ChangeState(LiHuo.battleState);
        }
    }
}
