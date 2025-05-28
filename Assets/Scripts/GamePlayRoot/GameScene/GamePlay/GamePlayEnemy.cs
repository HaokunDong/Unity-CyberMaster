using Cysharp.Text;
using Everlasting.Config;
using NodeCanvas.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayEnemy : GamePlayAIEntity
{
    private Cooldown MoveCD;
    private Cooldown CheckFaceFlipCD;
    private Cooldown ComboAttackCD;
    private Cooldown LeapAttackCD;
    private Cooldown StabAttackCD;

    public override void CustomAIBlackboardWriteIn(Blackboard blackboard)
    {
        base.CustomAIBlackboardWriteIn(blackboard);
        var data = EnemyTable.GetTableData(TableId);  

        if(data != null)
        {
            var gp = ManagerCenter.Ins.CooldownManager.GetOrCreateGroup(GamePlayId, UpdateMode.FixedUpdate);
            MoveCD = gp.Set("MoveCD", data.MoveCD);
            CheckFaceFlipCD = gp.Set("CheckFaceFlipCD", data.CheckFaceFlipTime);
            ComboAttackCD = gp.Set("ComboAttackCD", data.ComboAttackCD);
            LeapAttackCD = gp.Set("LeapAttackCD", data.LeapAttackCD);
            StabAttackCD = gp.Set("StabAttackCD", data.StabAttackCD);

            blackboard.SetVariableValue("CheckFaceFlipCD", CheckFaceFlipCD);
            blackboard.SetVariableValue("MoveCD", MoveCD);
            blackboard.SetVariableValue("ComboAttackCD", ComboAttackCD);
            blackboard.SetVariableValue("LeapAttackCD", LeapAttackCD);
            blackboard.SetVariableValue("StabAttackCD", StabAttackCD);
            blackboard.SetVariableValue("PlayerTooFarRange", data.PlayerTooFarRange);
            blackboard.SetVariableValue("AttackDistance", data.AttackDistance);
            blackboard.SetVariableValue("StabDistance", data.StabDistance);
            blackboard.SetVariableValue("MoveSpeed", data.MoveSpeed);
        }
    }

    public override void OnDispose()
    {
        base.OnDispose();
        ManagerCenter.Ins.CooldownManager.RemoveOwner(GamePlayId);
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat(GamePlayId, isGen ? " G " : " ", TableId);
        color = GamePlayId <= 0 ? Color.red : Color.yellow;
        return true;
    }
#endif
}
