using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBounceAttackState : PlayerState
{
    public PlayerBounceAttackState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (bounceAttackCounter > 1)
        {
            bounceAttackCounter = 0;
        }
        player.animator.SetInteger("BounceAttackCounter", bounceAttackCounter);
    }

    public override void Exit()
    {
        base.Exit();

        bounceAttackCounter++;
    }

    public override void Update()
    {
        base.Update();

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.postureState);
        }
    }
    public override void OnHit(Entity from)
    {
        HitEffectController.Create((from.transform.position + player.transform.position) / 2, new HitEffectInfo() { type = HitEffectType.BounceHit });
        player.info.life = Mathf.Clamp(player.info.life + GlobalRef.Ins.cfg.playerIncreaseLife_bounce, 0, 100);

    }
}
