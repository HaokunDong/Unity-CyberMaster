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
    public float attackCooldown;
    [HideInInspector] public float lastTimeAttacked;
    public int attackCount;
    public Vector2[] attackMovement;
    public float[] attackForce;

    private Transform playerTrans;

    public EnemyStateMachine stateMachine { get; private set; }


    protected override void Awake()
    {
        base.Awake();

        stateMachine = new EnemyStateMachine();

    }

    protected override void Start()
    {
        base.Start();

        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected override void Update()
    {
        base.Update();

        stateMachine.currentState.Update();
    }

    public virtual bool IsPlayerExist()
    {
        if (playerTrans != null)
        {
            return true;
        }
        return false;
    }

    public virtual int RelativePosition()//The RelativePosition with Player
    {
        if (this.transform.position.x - playerTrans.position.x < 0)
        {
            return 1;
        }
        else if (this.transform.position.x - playerTrans.position.x > 0)
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
        if (this.transform.position.x - playerTrans.position.x < 0)
        {
            return -(this.transform.position.x - playerTrans.position.x);
        }
        else
        {
            return this.transform.position.x - playerTrans.position.x;
        }

        //return Vector2.Distance(transform.position, playerTrans.position);
    }

    public virtual bool CanAttack()
    {
        if (Time.time >= lastTimeAttacked + attackCooldown)
        {
            return true;
        }
        return false;
    }

    //public virtual RaycastHit2D IsPlayerDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, 50f, whatIsPlayer);


    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance, transform.position.y));
    }

    public void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    public void PreProcessTrigger() => stateMachine.currentState.AnimationPreprocessTrigger();
}
