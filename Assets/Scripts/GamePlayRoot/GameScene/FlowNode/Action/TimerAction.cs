using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("GamePlay")]
public class TimerAction : ActionTask
{
    public BBParameter<float> time;

    protected override void OnUpdate()
    {
        base.OnUpdate();
        time.value += Time.deltaTime;
    }
}
