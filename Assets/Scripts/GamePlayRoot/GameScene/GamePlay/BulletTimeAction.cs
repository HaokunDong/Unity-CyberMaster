using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using HeaderAttribute = ParadoxNotion.Design.HeaderAttribute;

[Category("GamePlay")]
public class BulletTimeAction : ActionTask
{
    [Header("重要程度")]
    public BBParameter<int> weight;
    [Header("真实时间")]
    public BBParameter<float> durationRealtime;
    [Header("时间缩放")]
    public BBParameter<float> targetTimeScale;

    protected override void OnExecute()
    {
        var curve = AnimationCurve.Constant(0, 0, 1);
        BulletTimeTool.PlayBulletTime(weight.value, durationRealtime.value, curve, new Vector2(1f, targetTimeScale.value));
        EndAction(true);
    }
}
