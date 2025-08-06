using Cysharp.Text;
using Everlasting.Config;
using GameBase.Log;
using NodeCanvas.Framework;
using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayPlayer : GamePlayAIEntity, ISkillDriverUnit
{
    public float maxInteractDistance = 5f;
    //public Player player;

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
    private BetterJumping betterJumping;

    private Vector2 velocity;
    public Vector2 Velocity
    {
        get => velocity;
        private set
        {
            if(value != velocity)
            {
                velocity = value;
                blackboard.SetVariableValue("Velocity", velocity);
            }
        }
    }

    public GamePlayEntity skillDriverOwner => this;
    public SkillDriver skillDriverImp => skillDriver;
    public bool isOnGround => IsGrounded();

    public Vector2 GetFacingDirection()
    {
        return transform.right;
    }

    private void Awake()
    {
        playerInput ??= new PlayerInput();
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
    }

    private void Start()
    {
        coll = GetComponent<Collision>();
        betterJumping ??= new BetterJumping(0.5f, 3f, 8f, GetComponent<Rigidbody2D>(), playerInput);
    }

    public override void AfterAIInit(Blackboard blackboard)
    {
        base.AfterAIInit(blackboard);
        playerData = PlayerTable.GetTableData(TableId);
        blackboard.SetVariableValue("Velocity", Velocity);
        blackboard.SetVariableValue("SkillDriver", skillDriver);
        blackboard.SetVariableValue("beginSkill", false);
        blackboard.SetVariableValue("SkillPath", string.Empty);
        blackboard.SetVariableValue("isLanding", false);

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
        playerInput.GamePlay.Run.performed += OnRun;
        playerInput.GamePlay.Run.canceled += OnRun;

        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.GamePlay.Run.performed -= OnRun;
        playerInput.GamePlay.Run.canceled -= OnRun;
        playerInput.Disable();
    }

    private void OnRun(InputAction.CallbackContext context)
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
        betterJumping.Step();
        if (playerData != null)
        {
            moveVelocitySmoothDirectionInput.Update(Time.deltaTime);
            Velocity = new Vector2(moveVelocitySmoothDirectionInput.CurrentValue * playerData.MaxMoveSpeed, 0);

            attackInputButtonState.Update(Time.deltaTime);
            blockInputButtonState.Update(Time.deltaTime);

            if (!skillDriver.IsPlaying)
            {
                if (playerInput.GamePlay.Jump.IsPressed() && IsGrounded() && !blackboard.GetVariableValue<bool>("isLanding"))
                {
                    Jump(Vector2.up, false);
                }
                else
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
                                blackboard.SetVariableValue("beginSkill", true);
                                blackboard.SetVariableValue("SkillPath", ps);
                            }
                        }
                    }
                }
            }
        }

        if (ManagerCenter.Ins.PlayerInputMgr.CanGamePlayInput)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                World.Ins.InPlayGamePlayRoot?.InteractTarget?.OnInteract();
            }
        } 
    }

    private void Jump(Vector2 dir, bool wall)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * playerData.JumpForce;
    }

    public override void OnHitBoxTrigger(HitResType hitRestype, uint attackerGPId, uint beHitterGPId, float damageBaseValue)
    {
        base.OnHitBoxTrigger(hitRestype, attackerGPId, beHitterGPId, damageBaseValue);

        GamePlayEnemy enemy = World.Ins.GetRootByEntityId(attackerGPId).GetAGamePlayEntity<GamePlayEnemy>(attackerGPId);

        if(!isFacing(enemy))
        {
            Flip();
        }

        //player.OnHitFromTarget(attackerGPId);
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
