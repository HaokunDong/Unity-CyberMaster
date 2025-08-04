using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using Managers;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("GamePlay")]
[Name("释放技能")]
public class PlaySkillAction : ActionTask<ISkillDriverUnit>
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
                if(!agent.isOnGround && !skill.isAnAirSkill)
                {
                    EndAction(true);
                    return;
                }
                agent.skillDriverImp.OnSkillFinished += OnSkillFinished;
                agent.skillDriverImp.SetSkill(skill);
                agent.skillDriverImp.PlayAsync().Forget();
                return;
            }
        }
        EndAction(false);
    }

    private void OnSkillFinished()
    {
        agent.skillDriverImp.OnSkillFinished -= OnSkillFinished;
        EndAction(true);
    }

    protected override void OnStop()
    {
        agent.skillDriverImp.OnSkillFinished -= OnSkillFinished;
        if (agent.skillDriverImp.IsPlaying)
        {
            agent.skillDriverImp.Stop();
        }
    }
}
