using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoEndState : EnemyState
{
    protected Enemy_Boss_LiHuo liHuo;
    public LiHuoEndState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) 
        : base(_stateMachine, _enemy, _animBoolName)
    {
        liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();

        if (liHuo.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.simulated = false;
        }

        GameOverUIController.Instance.ShowVictory(); 
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
