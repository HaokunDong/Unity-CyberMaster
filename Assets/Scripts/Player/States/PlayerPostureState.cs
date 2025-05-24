using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPostureState : PlayerState
{
    public PlayerPostureState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam) : base(_stateMachine, _player, _animBoolNam)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.info.isBlock = true;

    }

    public override void Exit()
    {
        base.Exit();
        player.info.isBlock = false;
    }

    public override void Update()
    {
        base.Update();

        if (!Input.GetKey(KeyCode.Space))
        {
            stateMachine.ChangeState(player.idleState);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (player.canBounceOther)
            {
                stateMachine.ChangeState(player.bounceAttackState);
            }
        }
    }

    public override void OnHit(Entity from)
    {
        HitEffectController.Create((from.transform.position + player.transform.position) / 2f, new HitEffectInfo() { type = HitEffectType.BlockHit });
        player.info.life = Mathf.Clamp(player.info.life - GlobalRef.Ins.cfg.playerDecayLife_defense, 0, 100);

        if (player.facingDir != -from.facingDir)//Flip when player is attecked when player in posture.
        {
            player.Flip();
        }

        player.SetMovement(from.attackForce[from.attackCount], rb.velocity.y);

        stateMachine.ChangeState(player.postureMoveState);
    }
}
