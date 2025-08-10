using Cysharp.Text;
using Cysharp.Threading.Tasks;
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

    public GamePlayEntity skillDriverOwner => this;
    public SkillDriver skillDriverImp => skillDriver;

    [HideInInspector]
    public InputMovement movement;

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
        movement = GetComponent<InputMovement>();
        movement.Init(playerData, this, playerInput);
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
        return movement.IsGrounded();
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
                        movement.OnMoveInput(moveVelocitySmoothDirectionInput.CurrentValue, moveInput);
                    }
                }
            }
        }
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
