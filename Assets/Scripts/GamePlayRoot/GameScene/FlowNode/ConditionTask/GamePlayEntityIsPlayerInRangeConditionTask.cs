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
        if (World.Ins.Player != null)
        {
            if (needRaycastDetect.value)
            {
                var hit = Physics2D.Raycast(agent.transform.position, agent.transform.right, range.value);
                if (hit.collider != null && hit.collider.gameObject == World.Ins.Player.gameObject)
                {
                    return true;
                }

                return false;
            }
            else
            {
                float distance = Vector3.Distance(agent.transform.position, World.Ins.Player.transform.position);
                return distance <= range.value;
            }
        }
        return false;
    }
}
