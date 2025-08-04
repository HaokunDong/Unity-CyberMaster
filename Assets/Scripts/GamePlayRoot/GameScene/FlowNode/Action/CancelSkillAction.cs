using Cysharp.Threading.Tasks;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("GamePlay")]
[Name("取消技能")]
public class CancelSkillAction : ActionTask<ISkillDriverUnit>
{
    protected override void OnExecute()
    {
        base.OnExecute();
        agent.skillDriverImp.CancelSkill(true, true).Forget();
        EndAction(true);
    }
}
