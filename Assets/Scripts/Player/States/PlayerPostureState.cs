using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPostureState : PlayerState
{
    protected Enemy_Boss_LiHuo liHuo;
    const string onHitBeforeKey = "onHitBefore";
    const string onHitAfterKey = "onHitAfter";
    public PlayerPostureState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {

    }

    public override void Enter()
    {
        base.Enter();
        this.liHuo = EnemyManager.Ins.liHuo;

        postureTimer = 0;
        player.info.isBlock = true;

    }

    public override void Exit()
    {
        base.Exit();
        player.info.isBlock = false;
    }
    public override void OnHit(Entity from)
    {
        HitEffectController.Create((from.transform.position + player.transform.position) / 2f, new HitEffectInfo() { type = HitEffectType.BlockHit });
        player.info.life = Mathf.Clamp(player.info.life - GlobalRef.Ins.cfg.playerDecayLife_defense, 0, 100);
    }

    public override void Update()
    {
        base.Update();

        if (!Input.GetKey(KeyCode.Space))
        {
            stateMachine.ChangeState(player.idleState);
        }

        if (player.facingDir != player.RelativePosition())
        {
            player.FlipController(player.RelativePosition());
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll
                (liHuo.attackCheck[liHuo.attackCount].position, liHuo.attackCheckRadius[liHuo.attackCount]);

            foreach (var hit in colliders)
            {
                if (hit.GetComponent<Enemy_Boss_LiHuo>() != null)
                {
                    if (liHuo.canBeBouncedAttack)
                    {
                        stateMachine.ChangeState(player.bounceAttackState);
                    }
                }
            }
        }
    }
}
