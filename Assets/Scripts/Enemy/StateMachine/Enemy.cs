using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] protected LayerMask whatIsPlayer;
    [Header("Move Info")]
    public float moveSpeed = 3f;
    public float jumpForce = 12f;

    [Header("Attack Info")]
    public float attackDistance;
    public float nextTimeReadyToComboAttack;
    [SerializeField] public float ComboAttackCD;
    //public int attackCount;
    public Vector2[] attackMoveSpeed;
    //public float[] attackForce;

    //private Transform playerTrans;
    public GameObject playerPrefab;

    public EnemyStateMachine stateMachine { get; private set; }


    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();

    }

    protected override void Start()
    {
        base.Start();

        //playerTrans = PlayerManager.Ins.player.transform;
    }

    protected override void Update()
    {
        base.Update();

        stateMachine.currentState.Update();
    }

    public override void OnHitFromTarget(Entity from)
    {
        base.OnHitFromTarget(from);
        stateMachine.currentState.OnHit(from);
    }

    public override void BeExecution()
    {
        base.BeExecution();
        stateMachine.currentState.BeExecution();
    }

/*    public virtual bool IsInAttackRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll
                (attackCheck[attackCount].position, attackCheckRadius[attackCount]);
        foreach (var hit in colliders)
        {

            if (hit.GetComponent<Player>() != null)
            {
                return true;
            }
        }
        return false;
    }*/


    public virtual bool IsPlayerExist()
    {
        if (playerPrefab.gameObject != null)
        {
            return true;
        }
        return false;
    }

    public virtual int RelativePosition()//The RelativePosition with Player
    {
        if (this.transform.position.x - playerPrefab.gameObject.transform.position.x < 0)
        {
            return 1;
        }
        else if (this.transform.position.x - playerPrefab.gameObject.transform.position.x > 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public virtual float RelativeDistance()
    {
        if (this.transform.position.x - playerPrefab.gameObject.transform.position.x < 0)
        {
            return -(this.transform.position.x - playerPrefab.gameObject.transform.position.x);
        }
        else
        {
            return this.transform.position.x - playerPrefab.gameObject.transform.position.x;
        }

        //return Vector2.Distance(transform.position, playerTrans.position);
    }

    //public virtual RaycastHit2D IsPlayerDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, 50f, whatIsPlayer);


    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance, transform.position.y));
    }

    public void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    public void AttackMove() => stateMachine.currentState.AnimationEventTrigger();

}
