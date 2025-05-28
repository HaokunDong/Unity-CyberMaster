using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("GamePlay")]
public class PauseACooldownAction : ActionTask
{
    public BBParameter<Cooldown> cd;
    public BBParameter<bool> Pause = false;

    protected override void OnExecute()
    {
        base.OnExecute();
        if (cd.value != null)
        {
            cd.value.Paused = Pause.value;
        }
        EndAction(true);
    }
}
