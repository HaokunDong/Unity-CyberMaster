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
    protected override void Update()
    {
        base.Update();

        Debug.Log(stateMachine.currentState.ToString());
    }

    /*    protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            //Gizmos.DrawWireSphere(attackCheck[2].position, attackCheckRadius[2]);

            *//*        #region DrawStabAttack
            #if UNITY_EDITOR
                    Collider2D[] hits = Physics2D.OverlapBoxAll(stabAttackCheck.position, stabAttackSize, 0);
                    Gizmos.color = hits.Length > 0 ? Color.green : Color.white;
            #else
                        Gizmos.color = Color.blue;
            #endif
                    // 绘制旋转的矩形
                    Matrix4x4 rotationMatrix = Matrix4x4.TRS(stabAttackCheck.position, Quaternion.Euler(0, 0, 0), Vector3.one);
                    Gizmos.matrix = rotationMatrix;
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(stabAttackSize.x, stabAttackSize.y, 0));
                    Gizmos.matrix = Matrix4x4.identity; // 重置矩阵
                    #endregion*//*

        }*/

    public virtual bool IsPlayerInViewRange()
    {
        if(!player)
        {
            return false;
        }
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
