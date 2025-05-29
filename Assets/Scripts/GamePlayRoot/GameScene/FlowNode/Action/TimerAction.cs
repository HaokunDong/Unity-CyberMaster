using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("GamePlay")]
public class TimerAction : ActionTask
{
    public BBParameter<float> time;
    public BBParameter<bool> isAdd = true;

    protected override void OnUpdate()
    {
        base.OnUpdate();
        time.value += isAdd.value ? 1 : -1 * Time.deltaTime;
    }
}
