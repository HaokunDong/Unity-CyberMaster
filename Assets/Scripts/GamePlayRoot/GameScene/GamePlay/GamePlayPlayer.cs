using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using GameBase.Log;
using Sirenix.Utilities;
using System;
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
    private Camera mainCamera;

    public GamePlayEntity skillDriverOwner => this;
    public SkillDriver skillDriverImp => skillDriver;

    [HideInInspector]
    public NormalInputMovement normalMovement;

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
            OnHitBoxTrigger,
            () => Time.fixedDeltaTime,
            () => facingDir,
            () => { FacePlayer(); }
        );
        GetComponent<BetterJumping>().SetPlayerInput(playerInput);
        normalMovement = GetComponent<NormalInputMovement>();
        normalMovement.Init(playerData, this, playerInput);
    }

    private void Start()
    {
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

        mainCamera = Camera.main;
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
        return normalMovement.IsGrounded();
    }

    public float cr;
    public float vr;
    public Color fc;

    private void Update()
    {
        if(!skillDriver.IsPlaying && IsGrounded() && Mathf.Abs(normalMovement.rb.velocity.x) <= 0.02f)
        {
            FluidController.Ins.QueueDrawAtPoint(
                transform.position + (facingRight ? new Vector3(-0.4f, -0.3f, 0) : new Vector3(0.4f, -0.3f, 0)),
                fc,
                facingRight ? new Vector2(-0.42261826f, -0.90630779f) : new Vector2(0.42261826f, -0.90630779f),
                cr,
                vr,
                FluidController.VelocityType.Direct
            );
        }

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

                if(normalMovement.IsBeginDash(moveInput))//Dask能打断取消任意技能
                {
                    skillDriver.CancelSkill(false, true).Forget();
                }
                else
                {
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
                            normalMovement.OnMoveInput(moveVelocitySmoothDirectionInput.CurrentValue, moveInput);
                        }
                    }
                }
            }
        }
    }

    public override void OnHitBoxTrigger(HitResType hitRestype, uint attackerGPId, uint beHitterGPId, float damageBaseValue, Vector2 hitPoint)
    {
        GamePlayEnemy enemy = null;
        if (attackerGPId > 0)
        {
            enemy = World.Ins.GetRootByEntityId(attackerGPId).GetAGamePlayEntity<GamePlayEnemy>(attackerGPId);
        }
        else
        {
            enemy = World.Ins.GetRootByEntityId(beHitterGPId).GetAGamePlayEntity<GamePlayEnemy>(beHitterGPId);
        }

        switch(hitRestype)
        {
            case HitResType.EnemyHitPlayerBody:
                if(normalMovement.isDashing)
                {
                    //完美闪避
                    RippleController.Ins.AddRipple(hitPoint, mainCamera);
                    var curve = AnimationCurve.Constant(0, 0, 1);
                    BulletTimeTool.PlayBulletTime(3, 0.2f, curve, new Vector2(1f, 0.3f));
                }
                else if (skillDriver.IsPlayingABlockSkill())
                {
                    //普通格挡
                }
                else
                {
                    //受伤
                }
                break;
            case HitResType.EnemyHitPlayerBlock:
                //完美格挡
                break;
            case HitResType.PlayerEnemyBladeFight:
                BladeFightEffectController.Ins.StartBladeFightEffect(hitPoint, mainCamera, 0.3f, 0.4f);
                break;
            case HitResType.PlayerHitEnemyBody:
                break;
            case HitResType.PlayerHitEnemyBlock:
                break;
        }

        if(enemy != null && !isFacing(enemy))
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
