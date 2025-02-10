using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoBounceAttackState : EnemyState
{
    Enemy_Boss_LiHuo liHuo;
    public LiHuoBounceAttackState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) : base(_stateMachine, _enemy, _animBoolName)
    {
        this.liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();

        if (bounceAttackCounter > 1)
        {
            bounceAttackCounter = 0;
        }
        liHuo.animator.SetInteger("BounceAttackCounter", bounceAttackCounter);
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
            stateMachine.ChangeState(liHuo.idleState);
        }
    }
    public override void OnHit(Entity from)
    {
        base.OnHit(from);
        HitEffectController.Create((from.transform.position + enemy.transform.position) / 2f, new HitEffectInfo() { type = HitEffectType.BlockHit });
        if (from is Player)
        {
            Player p = from as Player;
            p.info.life = Mathf.Clamp(p.info.life + GlobalRef.Ins.cfg.playerIncreaseLife_attack, 0, 100);
            p.RefreshInfoState();
        }
    }
}
