using Cysharp.Text;
using Everlasting.Config;
using GameBase.Log;
using NodeCanvas.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

public class GamePlayEnemy : GamePlayAIEntity, ISkillDriverUnit
{
    public SkillDriver skillDriver;
    
    private Cooldown MoveCD;
    private Cooldown CheckFaceFlipCD;
    private Cooldown ComboAttackCD;
    private Cooldown LeapAttackCD;
    private Cooldown StabAttackCD;
    private Cooldown WithdrawAttackCD;

    public GamePlayEntity skillDriverOwner => this;
    public SkillDriver skillDriverImp => skillDriver;

    public override void AfterAIInit(Blackboard blackboard)
    {
        base.AfterAIInit(blackboard);
        var data = EnemyTable.GetTableData(TableId);  

        if(data != null)
        {
            var gp = ManagerCenter.Ins.CooldownMgr.GetOrCreateGroup(GamePlayId, UpdateMode.FixedUpdate);
            MoveCD = gp.Set("MoveCD", data.MoveCD);
            CheckFaceFlipCD = gp.Set("CheckFaceFlipCD", data.CheckFaceFlipTime);
            ComboAttackCD = gp.Set("ComboAttackCD", data.ComboAttackCD);
            LeapAttackCD = gp.Set("LeapAttackCD", data.LeapAttackCD);
            StabAttackCD = gp.Set("StabAttackCD", data.StabAttackCD);
            WithdrawAttackCD = gp.Set("WithdrawAttackCD", data.WithdrawAttackCD);

            blackboard.SetVariableValue("CheckFaceFlipCD", CheckFaceFlipCD);
            blackboard.SetVariableValue("MoveCD", MoveCD);
            blackboard.SetVariableValue("MoveCDTime", data.MoveCD);
            blackboard.SetVariableValue("ComboAttackCD", ComboAttackCD);
            blackboard.SetVariableValue("ComboAttackCDTime", data.ComboAttackCD);
            blackboard.SetVariableValue("LeapAttackCD", LeapAttackCD);
            blackboard.SetVariableValue("LeapAttackCDTime", data.LeapAttackCD);
            blackboard.SetVariableValue("StabAttackCD", StabAttackCD);
            blackboard.SetVariableValue("StabAttackCDTime", data.StabAttackCD);
            blackboard.SetVariableValue("WithdrawAttackCD", WithdrawAttackCD);
            blackboard.SetVariableValue("WithdrawAttackCDTime", data.WithdrawAttackCD);
            blackboard.SetVariableValue("PlayerTooFarRange", data.PlayerTooFarRange);
            blackboard.SetVariableValue("AttackDistance", data.AttackDistance);
            blackboard.SetVariableValue("StabDistance", data.StabDistance);
            blackboard.SetVariableValue("MoveSpeed", data.MoveSpeed);
            blackboard.SetVariableValue("PrimaryAttackSkillPath", data.PrimaryAttackSkillPath);
            blackboard.SetVariableValue("LeapAttackSkillPath", data.LeapAttackSkillPath);
            blackboard.SetVariableValue("StabAttackSkillPath", data.StabAttackSkillPath);
            blackboard.SetVariableValue("WithdrawAttackSkillPath", data.WithdrawAttackSkillPath);
            blackboard.SetVariableValue("BlockSkillOnePath", data.BlockSkillOnePath);
        }

        skillDriver = new SkillDriver(
            this,
            typeof(GamePlayEnemy),
            animator,
            rb,
            (HitResType hitRestype, uint attackerGPId, uint beHitterGPId, float damageBaseValue) => 
                {
                    LogUtils.Warning($"������������: {hitRestype} ������GPId: {attackerGPId} �ܻ���GPId: {beHitterGPId} �˺���׼ֵ: {damageBaseValue}");
                    World.Ins.Player.OnHitBoxTrigger(hitRestype, attackerGPId, beHitterGPId, damageBaseValue);
                },
            () => Time.fixedDeltaTime,
            () => facingDir,
            () => { FacePlayer(); }
        );
    }

    [Button("������ͣ����")]
    private void TestPause()
    {
        if(skillDriver.IsPaused)
        {
            skillDriver.Resume();
        }
        else
        {
            skillDriver.Pause();
        }
    }

    public override void SetAIActive(bool active)
    {
        if (!pauseWhenInvisible)
        {
            return;
        }
        if (active && !isAIActive)
        {
            isAIActive = true;
            graphOwner?.StartBehaviour();
            if (animator)
            {
                animator.enabled = true;
            }
            skillDriver.Resume();
        }
        else if (!active && isAIActive)
        {
            isAIActive = false;
            graphOwner?.PauseBehaviour();
            if (animator)
            {
                animator.enabled = false;
            }
            skillDriver.Pause();
        }
    }

    public override void OnDispose()
    {
        base.OnDispose();
        ManagerCenter.Ins.CooldownMgr.RemoveOwner(GamePlayId);
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat(GamePlayId, isGen ? " G " : " ", TableId);
        color = GamePlayId <= 0 ? Color.red : Color.yellow;
        return true;
    }

    void OnDrawGizmos()
    {
        if (skillDriver?.skillConfig?.SkillHitBoxData.currentClip == null || skillDriver?.skillConfig?.SkillHitBoxData.currentClip.HitBoxs.Count <= 0) return;

        Gizmos.color = Color.red;

        foreach (var box in skillDriver.skillConfig.SkillHitBoxData.currentClip.HitBoxs)
        {
            Vector3 center = transform.position + new Vector3(box.center.x * facingDir, box.center.y);
            Vector3 size = (Vector3)box.size;

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, box.rotation), Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }
#endif
}
