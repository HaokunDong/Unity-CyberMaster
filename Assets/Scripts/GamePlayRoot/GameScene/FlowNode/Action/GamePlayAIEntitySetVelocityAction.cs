using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("GamePlay")]
public class GamePlayAIEntitySetVelocityAction : ActionTask<GamePlayAIEntity>
{
    public BBParameter<Vector2> Velocity;

    protected override void OnExecute()
    {
        base.OnExecute();
        agent.SetVelocity(Velocity.value);
        EndAction(true);
    }
}
