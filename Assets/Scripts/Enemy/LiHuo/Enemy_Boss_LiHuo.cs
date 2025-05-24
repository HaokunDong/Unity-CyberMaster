using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTrees;

public class Enemy_Boss_LiHuo : Enemy
{
    //public Player player;

    [Header("LiHuoSkills Info")]
    private float nextTimeReadyToMove;
    [SerializeField] public float moveCD;

    private float nextTimeReadyToLeapAttack;
    [SerializeField] public float leapAttackCD;
    [SerializeField] public float leapAttackMoveSpeed;
    [SerializeField] public Transform leapAttackCheck;
    [SerializeField] public float leapAttackRadius;

    private float nextTimeReadyToStabAttack;
    [SerializeField] public float stabDistance;
    [SerializeField] public float stabAttackCD;
    [SerializeField] public float stabAttackMoveSpeed;
    [SerializeField] public Transform stabAttackCheck;
    [SerializeField] public Vector2 stabAttackSize;

    //public bool bossFightBegun;

    #region States
    public LiHuoAppearState appearState { get; private set; }
    public LiHuoIdleState idleState { get; private set; }
    public LiHuoMoveState moveState { get; private set; }
    public LiHuoBattleState battleState { get; private set; }
    public LiHuoCDState cdState { get; private set; }
    public LiHuoComboAttackState comboAttackState { get; private set; }
    public LiHuoPrimaryAttackState primaryAttackState { get; private set; }
    public LiHuoLeapAttackState leapAttackState { get; private set; }
    public LiHuoStabAttackState stabAttackState { get; private set; }
    public LiHuoBounceAttackState bounceAttackState { get; private set; }

    public LiHuoDeadState deadState { get; private set; }
    public LiHuoBeExecutedState beExecutedState { get; private set; }
    public LiHuoEndState endState { get; private set; }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        //player = PlayerManager.Ins.player;

        appearState = new LiHuoAppearState(stateMachine, this, "Appear", this);
        idleState = new LiHuoIdleState(stateMachine, this, "Idle", this);
        moveState = new LiHuoMoveState(stateMachine, this, "Move", this);
        battleState = new LiHuoBattleState(stateMachine, this, "Idle", this);
        cdState = new LiHuoCDState(stateMachine, this, "Idle", this);
        comboAttackState = new LiHuoComboAttackState(stateMachine, this, "Idle", this);
        primaryAttackState = new LiHuoPrimaryAttackState(stateMachine, this, "Attack", this);
        leapAttackState = new LiHuoLeapAttackState(stateMachine, this, "LeapAttack", this);
        stabAttackState = new LiHuoStabAttackState(stateMachine, this, "StabAttack", this);
        bounceAttackState = new LiHuoBounceAttackState(stateMachine, this, "BounceAttack", this);

        deadState = new LiHuoDeadState(stateMachine, this, "Dead", this);
        beExecutedState = new LiHuoBeExecutedState(stateMachine, this, "BeExecuted", this);
        endState = new LiHuoEndState(stateMachine, this, "End", this);

    }


    protected override void Start()
    {
        base.Start();

        moveSpeed = moveSpeed * 1.5f;

        Flip();

        stateMachine.Initialize(idleState);

    }

    protected override void Update()
    {
        base.Update();

        //Debug.Log(stateMachine.currentState.ToString());
    }

    public override void OnHitFromTarget(Entity from)
    {
        base.OnHitFromTarget(from);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        //Gizmos.DrawWireSphere(attackCheck[5].position, attackCheckRadius[5]);

/*        #region DrawStabAttack
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
        #endregion*/

        //Gizmos.DrawWireSphere(leapAttackCheck.position, leapAttackRadius);
    }



    #region DistanceJudge
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
    public bool IsPlayerInViewRange()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < 10)
        {
            return true;
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

    public bool IsPlayerInStabRange()
    {
        if (RaycastDetectPlayer(stabDistance))
        {
            return true;
        }
        return false;
    }

    public bool IsPlayerFar()
    {
        if (RelativeDistance() > 7.0f)
        {
            return true;
        }
        return false;
    }

    public bool IsPlayerClose()
    {
        if (RelativeDistance() <= 7.0f)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region CanUseSkill
    public bool CanMove()
    {
        if (Time.time >= nextTimeReadyToMove)
        {
            nextTimeReadyToMove = Time.time + moveCD;
            return true;
        }
        return false;
    }

    public bool CanComboAttack()
    {
        if (Time.time >= nextTimeReadyToComboAttack)
        {
            nextTimeReadyToComboAttack = Time.time + ComboAttackCD;
            return true;
        }
        return false;
    }

    public bool CanLeapAttack()
    {
        if (Time.time >= nextTimeReadyToLeapAttack)
        {
            nextTimeReadyToLeapAttack = Time.time + leapAttackCD;
            return true;
        }
        return false;
    }

    public bool CanStabAttack()
    {
        if (Time.time >= nextTimeReadyToStabAttack)
        {
            nextTimeReadyToStabAttack = Time.time + stabAttackCD;
            return true;
        }
        return false;
    }

    #endregion

    #region SkillTrigger
    public void StabAttackTrigger()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(stabAttackCheck.position, stabAttackSize, 0);
        foreach(var hit in hits)
        {
            if(hit.GetComponent<Player>() != null)
            {
                hit.GetComponent<Player>().OnHitFromTarget(this);
                HitTarget();
            }
        }
    }
    public void LeapAttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(leapAttackCheck.position, leapAttackRadius);
        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Player>() != null)
            {
                
                hit.GetComponent<Player>().OnHitFromTarget(this);
                HitTarget();
            }
        }
    }
    #endregion
}
