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

        
    }

    public override void OnHit(Entity from)
    {
        base.OnHit(from);

        if (player.canBeBouncedAttack)
        {
            stateMachine.ChangeState(liHuo.bounceAttackState);
        }

        //Play the HitEffect and sound effect clips
        HitEffectController.Create((from.transform.position + enemy.transform.position) / 2f, new HitEffectInfo() { type = HitEffectType.BlockHit });
        if (from is Player)
        {
            Player p = from as Player;
            p.info.life = Mathf.Clamp(p.info.life + GlobalRef.Ins.cfg.playerIncreaseLife_attack, 0, 100);
            p.RefreshInfoState();
        }
    }
}
