using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using GameBase.Log;
using Managers;
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
    private SkillConfig dashSkillConfig;
    private Animator animator = null;
    private int skillLayerIndex = -1;

    public GamePlayEntity skillDriverOwner => this;
    public SkillDriver skillDriverImp => skillDriver;

    [HideInInspector]
    public NormalInputMovement normalMovement;

    public bool isOnGround => IsGrounded();

    private void Awake()
    {
        animator ??= gameObject.GetComponentInChildren<Animator>();
        if(skillLayerIndex == -1)
        {
            skillLayerIndex = animator.GetLayerIndex("SkillLayer");
        }
        playerInput ??= new PlayerInput();
        playerData = PlayerTable.GetTableData(TableId);
        moveVelocitySmoothDirectionInput ??= new SmoothDirectionInput(1f, 3f, 0.1f, 0.05f);
        CustomTimeSystem.RegisterUnit(gameObject, TimeGroup.Player);
        skillDriver ??= new SkillDriver(
            this,
            typeof(GamePlayPlayer),
            animator,
            gameObject.GetComponentInChildren<Rigidbody2D>(),
            OnHitBoxTrigger,
            () => CustomTimeSystem.FixedDeltaTimeOf(TimeGroup.Player),
            () => facingDir,
            () => { FacePlayer(); }
        );
        GetComponent<BetterJumping>().SetPlayerInput(playerInput);
        normalMovement = GetComponent<NormalInputMovement>();
        normalMovement.Init(playerData, this, playerInput);
        LoadDaskSkill().Forget();
    }

    private async UniTask LoadDaskSkill()
    {
        dashSkillConfig = await ResourceManager.LoadAssetAsync<SkillConfig>("Skill/PlayerSkill/DashSkill", ResType.ScriptObject);
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

    public override void OnDispose()
    {
        base.OnDispose();
        CustomTimeSystem.UnregisterUnit(gameObject, TimeGroup.Player);
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
        if(normalMovement != null && normalMovement.rb != null && !skillDriver.IsPlaying && IsGrounded() && Mathf.Abs(normalMovement.rb.velocity.x) <= 0.02f)
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
                    async UniTask DashSkill()
                    {
                        await UniTask.DelayFrame(1);
                        skillDriver.ChangeSkillAsync(dashSkillConfig, false).Forget();
                    }
                    DashSkill().Forget();
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
                                    skillDriver.ChangeSkillAsync(ps, true).Forget();
                                }
                            }
                        }
                        else
                        {
                            if(normalMovement != null && normalMovement.rb != null)
                            {
                                normalMovement.OnMoveInput(moveVelocitySmoothDirectionInput.CurrentValue, moveInput);
                            }
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
            enemy = World.Ins.GetRootByEntityId(attackerGPId)?.GetAGamePlayEntity<GamePlayEnemy>(attackerGPId);
        }
        else
        {
            enemy = World.Ins.GetRootByEntityId(beHitterGPId)?.GetAGamePlayEntity<GamePlayEnemy>(beHitterGPId);
        }

        switch(hitRestype)
        {
            case HitResType.EnemyHitPlayerBody:
                if(skillDriver.HasActiveAttribute(GamePlayAttributeType.Invincible))
                {
                    //完美闪避
                    RippleController.Ins.AddRipple(hitPoint, mainCamera);
                    //var curve = AnimationCurve.Constant(0, 0, 1);
                    //BulletTimeTool.PlayBulletTime(3, 0.2f, curve, new Vector2(1f, 0.3f));

                    CustomTimeSystem.PlayBulletTime(
                        weight: 10,
                        group: TimeGroup.Enemy,
                        durationRealtime: 1f,
                        curve: AnimationCurve.EaseInOut(0, 0, 1, 1),
                        timeScaleRange: new Vector2(0.01f, 1f)
                    );
                }
                else if (skillDriver.IsPlayingABlockSkill())
                {
                    //普通格挡
                }
                else
                {
                    //受伤
                    var actionValue = enemy.skillDriver.GetAttributeValue(GamePlayAttributeType.ActionValue);
                    var saValue = skillDriver.GetAttributeValue(GamePlayAttributeType.SuperArmor);
                    if(saValue <= actionValue)
                    {
                        skillDriver.CancelSkill(false, true).Forget();
                        PlaySkillLayerAnim("PlayerBeAttacked").Forget();
                    }
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
                skillDriver.CancelSkill(false, true).Forget();
                PlaySkillLayerAnim("PlayerBeStunned").Forget();
                break;
        }

        if(enemy != null && !isFacing(enemy))
        {
            Flip();
        }
    }

    private async UniTask PlaySkillLayerAnim(string animName)
    {
        playerInput.Disable();
        await animator.PlayAndWaitAsync(animName, skillLayerIndex);
        animator.Play("Empty", skillLayerIndex, 0f);
        playerInput.Enable();
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
