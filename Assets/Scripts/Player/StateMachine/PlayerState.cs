using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected Player player;

    protected Rigidbody2D rb;

    protected float xInput;
    protected float yInput;
    private string animBoolName;

    protected float stateTimer;
    protected float postureTimer;

    protected bool triggerCalled;
    protected bool eventsTriggerCalled;


    public PlayerState(PlayerStateMachine _stateMachine, Player _player, string _animBoolNam)
    {
        this.stateMachine = _stateMachine;
        this.player = _player;
        this.animBoolName = _animBoolNam;
    }

    public virtual void Enter()
    {
        player.animator.SetBool(animBoolName, true);
        rb = player.rb;
        triggerCalled = false;
        eventsTriggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
        postureTimer += Time.deltaTime;

        if(ManagerCenter.Ins.PlayerInputMgr.CanGamePlayInput)
        {
            xInput = Input.GetAxisRaw("Horizontal");
            yInput = Input.GetAxisRaw("Vertical");
        }
        else
        {
            xInput = yInput = 0;
        }

        player.animator.SetFloat("yVelocity", rb.velocity.y);
        player.animator.SetFloat("PostureFloat", postureTimer);

    }

    public virtual void Exit()
    {
        player.animator.SetBool(animBoolName, false);
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }

    public virtual void AnimationEventTrigger()
    {
        eventsTriggerCalled = true;
    }

    public virtual void OnHit(Entity from) //受到攻击
    {
        //player.info.life = Mathf.Clamp(player.info.life - GlobalRef.Ins.cfg.playerDecayLife_hitted, 0, 100);

/*        if (player.facingDir != -from.facingDir)//Flip when player is attecked.
        {
            player.Flip();
        }*/
        Debug.Log("Player Be Attack!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        player.PlayerDead.AddListener(Dead);
        stateMachine.ChangeState(player.beAttackedState);
    }

    public virtual void Hit()
    {

    }

    public void Dead()
    {
        stateMachine.ChangeState(player.deadState);
    }
}
