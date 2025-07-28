using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using GameBase.Log;
using Managers;
using NodeCanvas.Framework;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayPlayer : GamePlayAIEntity
{
    public float maxInteractDistance = 5f;
    //public Player player;

    public PlayerInput playerInput;
    public Vector2 moveInput = Vector2.zero;

    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;
    private PlayerTable playerData;

    private Vector2 moveVelocity;
    private Vector2 runVelocity;
    public Vector2 MoveVelocity
    {
        get => moveVelocity;
        private set
        {
            if(value != moveVelocity)
            {
                moveVelocity = value;
                blackboard.SetVariableValue("MoveVelocity", MoveVelocity);
            }
        }
    }
    public Vector2 RunVelocity
    {
        get => runVelocity;
        private set
        {
            if (value != runVelocity)
            {
                runVelocity = value;
                blackboard.SetVariableValue("RunVelocity", RunVelocity);
            }
        }
    }


    public Vector2 GetFacingDirection()
    {
        return transform.right;
    }

    private void Awake()
    {
        playerInput ??= new PlayerInput();
    }

    private void Start()
    {
        //player = GetComponent<Player>();
    }

    public override void AfterAIInit(Blackboard blackboard)
    {
        base.AfterAIInit(blackboard);
        playerData = PlayerTable.GetTableData(TableId);
        blackboard.SetVariableValue("MoveVelocity", MoveVelocity);
        blackboard.SetVariableValue("RunVelocity", RunVelocity);
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

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        MoveVelocity = new Vector2(moveInput.x * playerData.MoveSpeed, 0);
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        RunVelocity = new Vector2(moveInput.x * playerData.RunSpeed, 0);
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

            if (Input.GetKeyDown(KeyCode.RightAlt))
            {
                TestBladeFightSkill().Forget();
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

    private async UniTask TestBladeFightSkill()
    {
        var skillDriver = new SkillDriver(
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

        var skill = await ResourceManager.LoadAssetAsync<SkillConfig>("Skill/TestPlayerBladeFight", ResType.ScriptObject);
        skillDriver.SetSkill(skill);
        skillDriver.PlayAsync().Forget();
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
