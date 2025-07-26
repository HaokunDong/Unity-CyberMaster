using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("Transform")]
public class GetPlayerRelativeDistanceXYAction : ActionTask<Transform>
{
    public BBParameter<bool> isCalX = true;
    public BBParameter<bool> isCalY = false;
    public BBParameter<float> res;

    protected override void OnExecute()
    {
        base.OnExecute();
        if (World.Ins.Player != null)
        {
            if(isCalX.value == isCalY.value)
            {
                res.value = Vector2.Distance(World.Ins.Player.transform.position, agent.position);
            }
            else
            {
                if (isCalX.value)
                {
                    res.value = Mathf.Abs(World.Ins.Player.player.transform.position.x - agent.position.x);
                }
                else
                {
                    res.value = Mathf.Abs(World.Ins.Player.transform.position.y - agent.position.y);
                }
            }
        }
        else
        {
            res.value = float.MaxValue;
        }
        EndAction(true);
    }
}
