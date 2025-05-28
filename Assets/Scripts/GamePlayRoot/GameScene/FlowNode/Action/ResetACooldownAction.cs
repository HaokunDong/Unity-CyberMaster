using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("GamePlay")]
public class ResetACooldownAction : ActionTask
{
    public BBParameter<Cooldown> cd;
    public BBParameter<bool> changeCDTime = false;
    public BBParameter<float> newCDTime = 1f;

    protected override void OnExecute()
    {
        base.OnExecute();
        if(cd.value != null)
        {
            if(changeCDTime.value)
            {
                cd.value.Reset(newCDTime.value);
            }
            else 
            {
                cd.value.Reset();
            }
        }
        EndAction(true);
    }
}
