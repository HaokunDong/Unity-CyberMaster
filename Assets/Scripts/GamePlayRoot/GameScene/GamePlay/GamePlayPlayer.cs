using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Everlasting.Config;
using GameBase.Log;
using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayPlayer : GamePlayEntity, ISkillDriverUnit
{
    public float maxInteractDistance = 5f;

    public PlayerInput playerInput;
    public Vector2 moveInput = Vector2.zero;

    private PlayerTable playerData;
    private SmoothDirectionInput moveVelocitySmoothDirectionInput;
    private Dictionary<CommandInputState, string> inputSkillDict;
    private SkillDriver skillDriver;
    private InputButtonState attackInputButtonState;
    private InputButtonState blockInputButtonState;
    private CommandInputState attackCMI;
    private CommandInputState blockCMI;
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;
    private GamePlayPlayerAnimationScript anim;

    [Space]
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;

    [Space]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;

    [Space]
    private bool groundTouch;
    private bool hasDashed;

    //[Space]
    //public ParticleSystem dashParticle;
    //public ParticleSystem jumpParticle;
    //public ParticleSystem wallJumpParticle;
    //public ParticleSystem slideParticle;

    public GamePlayEntity skillDriverOwner => this;
    public SkillDriver skillDriverImp => skillDriver;
    public bool isOnGround => IsGrounded();

    private void Awake()
    {
        playerInput ??= new PlayerInput();
        playerData = PlayerTable.GetTableData(TableId);
        moveVelocitySmoothDirectionInput ??= new SmoothDirectionInput(1f, 3f, 0.1f, 0.05f);
        skillDriver ??= new SkillDriver(
            this,
            typeof(GamePlayPlayer),
            gameObject.GetComponentInChildren<Animator>(),
            gameObject.GetComponentInChildren<Rigidbody2D>(),
            (HitResType hitRestype, uint attackerGPId, uint beHitterGPId, float damageBaseValue) =>
            {
                LogUtils.Warning($"攻击命中类型: {hitRestype} 攻击者GPId: {attackerGPId} 受击者GPId: {beHitterGPId} 伤害基准值: {damageBaseValue}");
            },
            () => Time.fixedDeltaTime,
            () => facingDir,
            () => { FacePlayer(); }
        );
        GetComponent<BetterJumping>().SetPlayerInput(playerInput);
    }

    private void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<GamePlayPlayerAnimationScript>();

        attackInputButtonState ??= new InputButtonState(playerInput.GamePlay.PrimaryAttack, InputCommand.Attack);
        blockInputButtonState ??= new InputButtonState(playerInput.GamePlay.Block, InputCommand.Block);

        if (inputSkillDict == null)
        {
            inputSkillDict = new();
            attackCMI = new CommandInputState { CMD = InputCommand.Attack, InputState = InputButtonFlags.JustPressed };
            inputSkillDict[attackCMI] = playerData.PrimaryAttackSkillPath;
            blockCMI = new CommandInputState { CMD = InputCommand.Block, InputState = InputButtonFlags.JustPressed };
            inputSkillDict[blockCMI] = playerData.BlockSkillPath;
        }
    }

    private void OnEnable()
    {
        playerInput.GamePlay.Run.performed += OnMoveInput;
        playerInput.GamePlay.Run.canceled += OnMoveInput;

        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.GamePlay.Run.performed -= OnMoveInput;
        playerInput.GamePlay.Run.canceled -= OnMoveInput;
        playerInput.Disable();
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        moveVelocitySmoothDirectionInput.SetRawInput(moveInput.x);
    }

    public bool IsGrounded()
    {
        return coll? coll.onGround : true;
    }

    private void Update()
    {
        if (playerData != null && ManagerCenter.Ins.PlayerInputMgr.CanGamePlayInput)
        {
            if (World.Ins.InPlayGamePlayRoot != null && World.Ins.InPlayGamePlayRoot.InteractTarget != null && playerInput.GamePlay.Interact.WasPressedThisFrame())
            {
                World.Ins.InPlayGamePlayRoot.InteractTarget.OnInteract();
            }
            else
            {
                moveVelocitySmoothDirectionInput.Update(Time.deltaTime);
                attackInputButtonState.Update(Time.deltaTime);
                blockInputButtonState.Update(Time.deltaTime);
                if (!skillDriver.IsPlaying)
                {
                    CommandInputState matchCMI = null;
                    if (attackInputButtonState.IsMatchAny(attackCMI))
                    {
                        matchCMI = attackCMI;
                    }
                    else if (blockInputButtonState.IsMatchAny(blockCMI))
                    {
                        matchCMI = blockCMI;
                    }
                    if (matchCMI != null)
                    {
                        if (inputSkillDict.TryGetValue(matchCMI, out var ps))
                        {
                            //在触发技能前就清空输入状态 防止连续触发多次技能
                            InputButtonState.GetButtonState(matchCMI.CMD)?.ClearFlagThisFrame();

                            if (!ps.IsNullOrWhitespace())
                            {
                                skillDriver.ChangeSkillAsync(ps).Forget();
                            }
                        }
                    }
                    else
                    {
                        float x = moveVelocitySmoothDirectionInput.CurrentValue;
                        float y = moveInput.y;
                        float xRaw = moveInput.x;
                        float yRaw = moveInput.y;
                        Vector2 dir = new Vector2(x, 0);

                        Walk(dir);
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
                                rb.velocity = new Vector2(rb.velocity.x, 0);

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
                            wallSlide = false;

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
                            if(xRaw == 0 && yRaw == 0)
                            {
                                Dash(facingDir, yRaw);
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

                        Flip(x);
                    }
                }
            }
        }
    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        //side = anim.sr.flipX ? -1 : 1;

        //jumpParticle.Play();
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
        GhostTrail.Ins.ShowGhost(this);
        GroundDash().Forget();
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        //dashParticle?.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        await UniTask.Delay(300);

        //dashParticle?.Stop();
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
        //if (coll.wallSide != side)
            //anim.Flip(side * -1);

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
            rb.velocity = new Vector2(dir.x * playerData.MaxMoveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * playerData.MaxMoveSpeed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        //slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        //ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * playerData.JumpForce;

        //particle.Play();
    }

    private async UniTask DisableMovement(float time)
    {
        canMove = false;
        await UniTask.Delay(System.TimeSpan.FromSeconds(time));
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    void WallParticle(float vertical)
    {
        //var main = slideParticle.main;

        if (wallSlide || (wallGrab && vertical < 0))
        {
            //slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            //main.startColor = Color.white;
        }
        else
        {
            //main.startColor = Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }

    public override void OnHitBoxTrigger(HitResType hitRestype, uint attackerGPId, uint beHitterGPId, float damageBaseValue)
    {
        base.OnHitBoxTrigger(hitRestype, attackerGPId, beHitterGPId, damageBaseValue);

        GamePlayEnemy enemy = World.Ins.GetRootByEntityId(attackerGPId).GetAGamePlayEntity<GamePlayEnemy>(attackerGPId);

        if(!isFacing(enemy))
        {
            Flip();
        }
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat(GamePlayId);
        color = Color.red;
        return true;
    }
#endif
}
