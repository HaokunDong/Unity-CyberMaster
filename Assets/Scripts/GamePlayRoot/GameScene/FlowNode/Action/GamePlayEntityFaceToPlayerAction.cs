using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("GamePlay")]
public class GamePlayEntityFaceToPlayerAction : ActionTask<GamePlayEntity>
{
    protected override void OnExecute()
    {
        base.OnExecute();
        agent.FacePlayer();
        EndAction(true);
    }
}
