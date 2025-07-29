using Cysharp.Text;
using Everlasting.Config;
using GameBase.Log;
using NodeCanvas.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayPlayer : GamePlayAIEntity, ISkillDriverUnit
{
    public float maxInteractDistance = 5f;
    //public Player player;

    public PlayerInput playerInput;
    public Vector2 moveInput = Vector2.zero;

    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;
    private PlayerTable playerData;
    private SmoothDirectionInput moveVelocitySmoothDirectionInput;
    private Dictionary<InputAction, string[]> inputSkillDict;
    private SkillDriver skillDriver;

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
                blackboard.SetVariableValue("AbsVelocityX", Mathf.Abs(Velocity.x));
            }
        }
    }

    public SkillDriver skillDriverImp => skillDriver;

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
        //player = GetComponent<Player>();
    }

    public override void AfterAIInit(Blackboard blackboard)
    {
        base.AfterAIInit(blackboard);
        playerData = PlayerTable.GetTableData(TableId);
        blackboard.SetVariableValue("Velocity", Velocity);
        blackboard.SetVariableValue("AbsVelocityX", Mathf.Abs(Velocity.x));
        blackboard.SetVariableValue("SkillDriver", skillDriver);
        blackboard.SetVariableValue("beginSkill", false);
        blackboard.SetVariableValue("SkillPath", string.Empty);

        if (inputSkillDict == null)
        {
            inputSkillDict = new();
            inputSkillDict[playerInput.GamePlay.PrimaryAttack] = playerData.PrimaryAttackSkillPath;
            inputSkillDict[playerInput.GamePlay.Block] = new string[] { playerData.BlockSkillPath };
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

    private void FixedUpdate()
    {
        if(playerData != null)
        {
            moveVelocitySmoothDirectionInput.Update(Time.fixedDeltaTime);
            Velocity = new Vector2(moveVelocitySmoothDirectionInput.CurrentValue * playerData.MaxMoveSpeed, 0);

            InputAction currentSkillAction = null;
            if (playerInput.GamePlay.Block.WasPressedThisFrame())
            {
                currentSkillAction = playerInput.GamePlay.Block;
            }

            if(playerInput.GamePlay.PrimaryAttack.WasPressedThisFrame())
            {
                currentSkillAction = playerInput.GamePlay.PrimaryAttack;
            }

            if(currentSkillAction != null)
            {
                if (inputSkillDict.TryGetValue(currentSkillAction, out var ps))
                {
                    if (ps != null && ps.Length > 0)
                    {
                        blackboard.SetVariableValue("beginSkill", true);
                        blackboard.SetVariableValue("SkillPath", ps[0]);
                    }
                }
            }
        }
    }

    public bool IsGrounded()
    {
        return Physics2D.BoxCast(groundCheckPoint.position, groundCheckSize, 0f, Vector2.zero, 0f, groundLayer);
    }

    private void Update()
    {
        if(SkillBoxManager.IsACharacterBeHitThisFrame(0))
        {
            LogUtils.Error("Hit", LogChannel.Battle, Color.green);
        }
        if (ManagerCenter.Ins.PlayerInputMgr.CanGamePlayInput)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                World.Ins.InPlayGamePlayRoot?.InteractTarget?.OnInteract();
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

        //player.OnHitFromTarget(attackerGPId);
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat(GamePlayId);
        color = Color.red;
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(groundCheckPoint.position, groundCheckSize);
    }
#endif
}
