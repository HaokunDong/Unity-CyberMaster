using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoGroundState : EnemyState
{
    protected Player player;
    protected Enemy_Boss_LiHuo liHuo;
    public LiHuoGroundState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) : base(_stateMachine, _enemy, _animBoolName)
    {
        this.liHuo = _liHuo;
        this.player = PlayerManager.Ins.player;
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

        Collider2D[] colliders = Physics2D.OverlapCircleAll
            (player.attackCheck[player.attackCount].position, player.attackCheckRadius[player.attackCount]);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy_Boss_LiHuo>() != null)
            {
                if (player.gameObject != null && player.canBeBouncedAttack)
                {
                    stateMachine.ChangeState(liHuo.bounceAttackState);
                }
            }
        }
    }
}
