using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("GamePlay")]
public class GamePlayEntityIsPlayerInRangeConditionTask : ConditionTask<GamePlayEntity>
{

    public BBParameter<float> range;
    public BBParameter<bool> needRaycastDetect = false;

    protected override string info => $" Is Player in {agentInfo} Range {range.value}";
    protected override bool OnCheck()
    {
        if(GamePlayRoot.Current != null)
        {
            if(GamePlayRoot.Current.player != null)
            {
                if(needRaycastDetect.value)
                {
                    var hit = Physics2D.Raycast(agent.transform.position, agent.transform.right, range.value);
                    if (hit.collider != null && hit.collider.gameObject == GamePlayRoot.Current.player.gameObject)
                    {
                        return true;
                    }

                    return false;
                }
                else
                {
                    float distance = Vector3.Distance(agent.transform.position, GamePlayRoot.Current.player.transform.position);
                    return distance <= range.value;
                }
            }
        }
        return false;
    }
}
