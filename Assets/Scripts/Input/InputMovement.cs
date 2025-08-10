using Cysharp.Threading.Tasks;
using DG.Tweening;
using Everlasting.Config;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class InputMovement : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody2D rb;

    [Space]
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;

    [Space]
    [ShowInInspector, ReadOnly, NonSerialized]
    public bool canMove = true;
    [ShowInInspector, ReadOnly, NonSerialized]
    public bool wallGrab;
    [ShowInInspector, ReadOnly, NonSerialized]
    public bool wallJumped;
    [ShowInInspector, ReadOnly, NonSerialized]
    public bool wallSlide;
    [ShowInInspector, ReadOnly, NonSerialized]
    public bool isDashing;
    [ShowInInspector, ReadOnly, NonSerialized]
    public bool isInPushing;
    [ShowInInspector, ReadOnly, NonSerialized]
    public bool groundTouch;
    [ShowInInspector, ReadOnly, NonSerialized]
    public bool hasDashed;

    [Space]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;

    private Collision coll;
    private GamePlayPlayerAnimationScript anim;
    private PlayerTable playerData;
    private GamePlayEntity gpEntity;
    private PlayerInput playerInput;

    private void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<GamePlayPlayerAnimationScript>();
    }

    public void Init(PlayerTable data, GamePlayEntity e, PlayerInput inp)
    {
        playerData = data;
        gpEntity = e;
        playerInput = inp;
    }

    public bool IsGrounded()
    {
        return coll ? coll.onGround : true;
    }

    public void OnMoveInput(float sx, Vector2 mi)
    {
        float x = sx;
        float y = mi.y;
        if (Mathf.Approximately(x * x + y * y, 1) && !Mathf.Approximately(x, 0) && IsGrounded())
        {
            x = x > 0 ? 1 : -1;
        }
        float xRaw = mi.x;
        float yRaw = mi.y;
        Vector2 dir = new Vector2(x, y);
        if (!isInPushing)
        {
            Walk(dir);
        }
        anim?.SetHorizontalMovement(x, y, rb ? rb.velocity.y : 0);

        //if (coll.onWall && Input.GetButton("Fire3") && canMove)
        //{
        //    if (facingDir != coll.wallSide)
        //        Flip();
        //    wallGrab = true;
        //    wallSlide = false;
        //}

        //if (Input.GetButtonUp("Fire3") || !coll.onWall || !canMove)
        //{
        //    wallGrab = false;
        //    wallSlide = false;
        //}

        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }

        if (wallGrab && !isDashing)
        {
            rb.gravityScale = 0;
            if (x > .2f || x < -.2f)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }

            float speedModifier = y > 0 ? .5f : 1;
            rb.velocity = new Vector2(rb.velocity.x, y * (playerData.MaxMoveSpeed * speedModifier));
        }
        else
        {
            rb.gravityScale = 3;
        }

        if (coll.onWall && !coll.onGround)
        {
            if (x != 0 && !wallGrab)
            {
                wallSlide = true;
                WallSlide();
            }
        }

        if (!coll.onWall || coll.onGround)
        {
            wallSlide = false;
        }

        if (playerInput.GamePlay.Jump.WasPressedThisFrame())
        {
            anim.SetTrigger("jump");

            if (coll.onGround)
                Jump(Vector2.up, false);
            if (coll.onWall && !coll.onGround)
                WallJump();
        }

        if (playerInput.GamePlay.Dash.WasPressedThisFrame() && !hasDashed)
        {
            if (xRaw == 0 && yRaw == 0)
            {
                Dash(gpEntity.facingDir, yRaw);
            }
            else
            {
                Dash(xRaw, yRaw);
            }
        }

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if (!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        WallParticle(y);

        if (wallGrab || wallSlide || !canMove)
            return;

        gpEntity.Flip(x);
    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        //side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }

    private void Dash(float x, float y)
    {
        RippleController.Ins.AddRipple(transform.position, Camera.main);

        hasDashed = true;
        anim.SetTrigger("dash");
        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.velocity += dir.normalized * playerData.DashSpeed;
        DashWait().Forget();
    }

    private async UniTask DashWait()
    {
        GhostTrail.Ins.ShowGhost(gpEntity);
        GroundDash().Forget();
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle?.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        await UniTask.Delay(300);

        dashParticle?.Stop();
        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    private async UniTask GroundDash()
    {
        await UniTask.Delay(150);
        if (coll.onGround)
        {
            hasDashed = false;
        }
    }

    private void WallJump()
    {
        //if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        //{
        //    side *= -1;
        //    anim.Flip(side);
        //}

        DisableMovement(0.1f).Forget();

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        if (coll.wallSide != gpEntity.facingDir)
            gpEntity.Flip();

        if (!canMove)
            return;

        bool pushingWall = false;
        if ((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;
        rb.velocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            if (!IsGrounded() && !wallGrab && !wallSlide)
            {
                if (coll.ChenckAround())
                {
                    //一点点地方蹭到墙，但是又不形成抓墙滑墙的时候不响应输入值，防止凌空卡墙上
                    return;
                }
            }
            rb.velocity = new Vector2(dir.x * playerData.MaxMoveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * playerData.MaxMoveSpeed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * playerData.JumpForce;

        particle.Play();
    }

    private async UniTask DisableMovement(float time)
    {
        canMove = false;
        await UniTask.Delay(System.TimeSpan.FromSeconds(time));
        canMove = true;
    }

    private void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    private void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    private int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("EnemyHead"))
        {
            isInPushing = true;
            Vector2 direction = new Vector2(transform.position.x > other.transform.position.x ? 2f : -2f, -1f);
            rb.velocity = direction * playerData.JumpForce;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("EnemyHead"))
        {
            isInPushing = false;
        }
    }
}
