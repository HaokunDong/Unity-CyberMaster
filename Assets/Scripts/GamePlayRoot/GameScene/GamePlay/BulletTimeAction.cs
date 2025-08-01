using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("GamePlay")]
public class BulletTimeAction : ActionTask
{
    public BBParameter<float> durationRealtime;
    public BBParameter<float> targetTimeScale;

    protected override void OnExecute()
    {
        var curve = AnimationCurve.Constant(0, 0, 1);
        BulletTimeTool.PlayBulletTime(durationRealtime.value, curve, new Vector2(1f, targetTimeScale.value));
        EndAction(true);
    }
}
