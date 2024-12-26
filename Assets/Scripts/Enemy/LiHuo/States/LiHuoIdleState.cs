using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoIdleState : EnemyState
{
    private Enemy_Boss_LiHuo _liHuo;

    public LiHuoIdleState(EnemyStateMachine stateMachine, Enemy enemy, string animBoolName, Enemy_Boss_LiHuo liHuo) : base(stateMachine, enemy, animBoolName)
    {
        this._liHuo = liHuo;
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
