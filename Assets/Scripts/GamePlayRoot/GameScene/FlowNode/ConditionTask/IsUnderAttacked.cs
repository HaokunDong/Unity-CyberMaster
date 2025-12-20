using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("GamePlay")]
public class IsUnderAttacked : ConditionTask<GamePlayEntity>
{
    //public BBParameter<bool> isAttacked = false;

    //protected override string info => $" Is Player {agentInfo} under attack ";


    protected override bool OnCheck()
    {

        if(agent.GetHitBoxTriggerCalled())
        {
            return true;
        }
        return false;

    }

}
