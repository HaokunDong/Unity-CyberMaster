using Cysharp.Threading.Tasks;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Name("等待后执行的Action")]
public class WaitDoAction : ActionTask
{
    public BBParameter<ActionTask> task;
    public BBParameter<float> waitTime = 0.1f;
    protected override void OnExecute()
    {
        base.OnExecute();
        Do().Forget();

    }

    private async UniTaskVoid Do()
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime.value));
        task?.value.Execute(agent, blackboard);
        EndAction(true);
    }
}
