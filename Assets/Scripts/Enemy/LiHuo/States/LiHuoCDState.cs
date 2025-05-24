using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoCDState : EnemyState
{
    protected Enemy_Boss_LiHuo liHuo;
    public LiHuoCDState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo) 
        : base(_stateMachine, _enemy, _animBoolName)
    {
        liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = 0.7f;

    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if(liHuo.facingDir != liHuo.RelativePosition() && stateTimer <= 0)
        {
            liHuo.FlipController(liHuo.RelativePosition());
        }

        if (Time.time >= liHuo.nextTimeReadyToComboAttack && liHuo.facingDir == liHuo.RelativePosition()) {
            stateMachine.ChangeState(liHuo.battleState);
        }

        PlayerManager.Ins.player.OnExecution.AddListener(Dead);
    }

    public override void OnHit(Entity from)
    {
        base.OnHit(from);

        liHuo.SetMovement(from.attackForce[from.attackCount], rb.velocity.y);

        if (liHuo.canBounceOther)
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

    public void Dead()
    {
        stateMachine.ChangeState(liHuo.deadState);
    }

}
