using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameBase.Log;
using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayPlayer : GamePlayEntity
{
    public float maxInteractDistance = 5f;
    public Player player;

    public Vector2 GetFacingDirection()
    {
        return transform.right;
    }

    private void Start()
    {
        player = GetComponent<Player>();
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
                GamePlayRoot.Current?.InteractTarget?.OnInteract();
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

        GamePlayEnemy enemy = GamePlayRoot.Current.GetAGamePlayEntity<GamePlayEnemy>(attackerGPId);

        if(player.facingDir == enemy.facingDir)
        {
            player.Flip();
        }

        player.OnHitFromTarget(attackerGPId);
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
#endif
}
