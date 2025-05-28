using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("GamePlay")]
public class IsACooldownReadyConditionTask : ConditionTask
{
    public BBParameter<Cooldown> cd;
    protected override string info => $" Is {cd} Ready";
    protected override bool OnCheck()
    {
        if (cd.value != null)
        {
            return cd.value.IsReady;
        }
        return false;
    }
}
