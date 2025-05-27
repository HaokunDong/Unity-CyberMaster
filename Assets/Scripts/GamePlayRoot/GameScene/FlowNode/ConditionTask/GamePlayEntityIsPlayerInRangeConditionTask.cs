using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("GamePlay")]
public class GamePlayEntityIsPlayerInRangeConditionTask : ConditionTask<GamePlayEntity>
{

    public BBParameter<float> range;

    protected override string info => $" Is Player in Range {range.value}";
    protected override bool OnCheck()
    {
        if(GamePlayRoot.Current != null)
        {
            if(GamePlayRoot.Current.player != null)
            {
                float distance = Vector3.Distance(agent.transform.position, GamePlayRoot.Current.player.transform.position);
                return distance <= range.value;
            }
        }
        return false;
    }
}
