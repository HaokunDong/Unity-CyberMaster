using Cysharp.Text;
using Everlasting.Config;
using NodeCanvas.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayEnemy : GamePlayAIEntity
{
    public override void CustomAIBlackboardWriteIn(Blackboard blackboard)
    {
        base.CustomAIBlackboardWriteIn(blackboard);
        var data = EnemyTable.GetTableData(TableId);
        if(data != null)
        {
            blackboard.SetVariableValue("CheckFaceFlipTime", data.CheckFaceFlipTime);
            blackboard.SetVariableValue("MoveCD", data.MoveCD);
            blackboard.SetVariableValue("ComboAttackCD", data.ComboAttackCD);
            blackboard.SetVariableValue("LeapAttackCD", data.LeapAttackCD);
            blackboard.SetVariableValue("StabAttackCD", data.StabAttackCD);
            blackboard.SetVariableValue("PlayerTooFarRange", data.PlayerTooFarRange);
        }
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
