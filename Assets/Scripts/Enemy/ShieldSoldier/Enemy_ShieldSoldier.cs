using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ShieldSoldier : Enemy
{
    #region States
    public ShieldSoldierIdleState idleState { get; private set; }
    public ShieldSoldierRunState runState { get; private set; }

    public ShieldSoldierPrimaryAttackState primaryAttackState { get; private set; }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        idleState = new ShieldSoldierIdleState(stateMachine, this, "Idle", this);
        runState = new ShieldSoldierRunState(stateMachine, this, "Run", this);

        primaryAttackState = new ShieldSoldierPrimaryAttackState(stateMachine, this, "Attack", this);
    }
    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);

    }

    public bool IsPlayerInViewRange()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < 10)
        {
            return true;
        }
        return false;
    }
    public bool RaycastDetectPlayer(float DetectDistance)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.right, DetectDistance);
        foreach (var hit in hits)
        {
            if (hit.collider.GetComponent<Player>() != null)
            {
                return true;
            }
        }

        return false;
    }
    public bool IsEnterAttackState()//Enter Attack State
    {
        if (RaycastDetectPlayer(attackDistance - 1))
        {
            return true;
        }
        return false;
    }
    public bool IsPlayerInAttackRange()//After enter attack state
    {
        if (RaycastDetectPlayer(attackDistance))
        {
            return true;
        }
        return false;
    }
}
