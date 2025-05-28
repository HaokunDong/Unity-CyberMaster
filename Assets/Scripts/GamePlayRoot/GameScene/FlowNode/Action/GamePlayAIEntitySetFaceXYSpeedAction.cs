using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShowIfAttribute = Sirenix.OdinInspector.ShowIfAttribute;

[Category("GamePlay")]
public class GamePlayAIEntitySetFaceXYSpeedAction : ActionTask<GamePlayAIEntity>
{
    public BBParameter<bool> changeSpeedX = true;
    [ShowIf("@changeSpeedX.value")]
    public BBParameter<float> speedXValue = 1;
    public BBParameter<bool> changeSpeedY = false;
    [ShowIf("@changeSpeedY.value")]
    public BBParameter<float> speedYValue = 1;

    protected override void OnExecute()
    {
        base.OnExecute();
        agent.SetVelocity(new Vector2(agent.facingDir * (changeSpeedX.value ? speedXValue.value : agent.rb.velocity.x), changeSpeedY.value ? speedYValue.value : agent.rb.velocity.y));
        EndAction(true);
    }
}
