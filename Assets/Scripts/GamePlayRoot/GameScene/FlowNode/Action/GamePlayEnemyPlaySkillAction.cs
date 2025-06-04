using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using Managers;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("GamePlay")]
public class GamePlayEnemyPlaySkillAction : ActionTask<GamePlayEnemy>
{
    public BBParameter<string> skillPath;

    protected override void OnExecute()
    {
        base.OnExecute();
        PlaySkill().Forget();
    }

    private async UniTaskVoid PlaySkill()
    {
        if(!skillPath.value.IsNullOrEmpty())
        {
            var skill = await ResourceManager.LoadAssetAsync<SkillConfig>(skillPath.value, ResType.ScriptObject);
            if(skill != null)
            {
                agent.skillDriver.OnSkillFinished += OnSkillFinished;
                agent.skillDriver.SetSkill(skill);
                agent.skillDriver.PlayAsync().Forget();
                return;
            }
        }
        EndAction(false);
    }

    private void OnSkillFinished()
    {
        agent.skillDriver.OnSkillFinished -= OnSkillFinished;
        EndAction(true);
    }

    protected override void OnStop()
    {
        agent.skillDriver.OnSkillFinished -= OnSkillFinished;
        if (agent.skillDriver.IsPlaying)
        {
            agent.skillDriver.Stop();
        }
    }
}
