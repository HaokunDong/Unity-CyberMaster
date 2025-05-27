using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("GamePlay")]
public class GamePlayEntityIsFacingPlayerConditionTask : ConditionTask<GamePlayEntity>
{
    protected override string info => agentInfo + " is Facing Player";
    protected override bool OnCheck()
    {
        return agent.isFacingPlayer();
    }
}
