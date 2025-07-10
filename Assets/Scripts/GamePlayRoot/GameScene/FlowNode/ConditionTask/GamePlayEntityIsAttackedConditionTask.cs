using GameBase.Log;
using NodeCanvas.Framework;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Category("GamePlay")]
public class GamePlayEntityIsAttackedConditionTask : ConditionTask<GamePlayEntity>
{
    protected override bool OnCheck()
    {
        if (SkillBoxManager.IsACharacterBeHitThisFrame(35001))
        {
            Debug.Log("HIT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            return true;
        }
        return false;
    }
}
