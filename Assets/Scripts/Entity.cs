using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Collision Info")]
    [SerializeField] public Transform[] attackCheck;
    [SerializeField] public float[] attackCheckRadius;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance;
    [SerializeField] protected LayerMask whatIsGround;//���ü��������ǵ���
    [Header("BounceAttack Info")]
    [SerializeField] public bool canBeBouncedAttack;
    [SerializeField] public GameObject canBeBouncedImage;

    #region Components
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }
    #endregion

    public int facingDir { get; private set; } = 1;
    protected bool facingRight = true;

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {

    }

    public virtual void CanBeBouncedAttack()
    {
        canBeBouncedAttack = true;
        canBeBouncedImage.SetActive(true);
    }

    public virtual void CanNotBeBouncedAttack()
    {
        canBeBouncedAttack = false;
        canBeBouncedImage.SetActive(false);
    }

    public virtual void HitTarget(Entity from)
    {
        //Debug.Log(gameObject.name + "hit");
    }

    #region Velocity
    public virtual void SetZeroVelocity() => rb.velocity = new Vector2(0, 0);

    public virtual void SetVelocity(float _xVelocity, float _yVelocity)
    {
        FlipController(_xVelocity);
        rb.velocity = new Vector2(_xVelocity, _yVelocity);
    }

    public virtual void SetMovement(float _xVelocity, float _yVelocity)
    {
        rb.velocity = new Vector2(_xVelocity, _yVelocity);
    }
    #endregion
    #region Collision
    public virtual bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);//�ڶ�������Ҫ���Ϸ�������

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance * facingDir, wallCheck.position.y));
    }
    #endregion
    #region Flip
    public virtual void Flip()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    public virtual void FlipController(float _x)
    {
        if (_x > 0 && !facingRight)
        {
            Flip();
        }
        else if (_x < 0 && facingRight)
        {
            Flip();
        }
    }
    #endregion

}
