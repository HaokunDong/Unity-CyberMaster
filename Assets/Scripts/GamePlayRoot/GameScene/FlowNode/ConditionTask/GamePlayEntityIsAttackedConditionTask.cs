using GameBase.Log;
using NodeCanvas.Framework;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Category("GamePlay")]
public class GamePlayEntityIsAttackedConditionTask : ConditionTask<GamePlayEntity>
{
    protected override string info => agentInfo + " is Attacked";

    protected override bool OnCheck()
    {
        if (SkillBoxManager.IsACharacterBeHitThisFrame(agent.GamePlayId))
        {
            Debug.Log("HIT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            return true;
        }
        Debug.Log("FALSE!!!");
        return false;
    }
}
