using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoGroundState : EnemyState
{
    protected Enemy_Boss_LiHuo liHuo;
    public LiHuoGroundState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo)
        : base(_stateMachine, _enemy, _animBoolName)
    {
        liHuo = _liHuo;
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

    }

}
