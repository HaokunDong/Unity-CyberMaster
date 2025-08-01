using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("GamePlay")]
public class SwitchSkillDiverHitResTypeAction : ActionTask<ISkillDriverUnit>
{
    public BBParameter<ActionTask> EnemyHitPlayerBodyTask;
    public BBParameter<ActionTask> EnemyHitPlayerBlockTask;
    public BBParameter<ActionTask> PlayerEnemyBladeFightTask;
    public BBParameter<ActionTask> PlayerHitEnemyBodyTask;
    public BBParameter<ActionTask> PlayerHitEnemyBlockTask;

    protected override void OnExecute()
    {
        if(agent != null && agent.skillDriverImp != null)
        {
            switch (agent.skillDriverImp.HitResTp)
            {
                case HitResType.EnemyHitPlayerBody:
                    if (!EnemyHitPlayerBodyTask.isNoneOrNull)
                    {
                        EnemyHitPlayerBodyTask.value.Execute(agent.skillDriverOwner, blackboard);
                    }
                    break;
                case HitResType.EnemyHitPlayerBlock:
                    if (!EnemyHitPlayerBlockTask.isNoneOrNull)
                    {
                        EnemyHitPlayerBlockTask.value.Execute(agent.skillDriverOwner, blackboard);
                    }
                    break;
                case HitResType.PlayerEnemyBladeFight:
                    if (!PlayerEnemyBladeFightTask.isNoneOrNull)
                    {
                        PlayerEnemyBladeFightTask.value.Execute(agent.skillDriverOwner, blackboard);
                    }
                    break;
                case HitResType.PlayerHitEnemyBody:
                    if (!PlayerHitEnemyBodyTask.isNoneOrNull)
                    {
                        PlayerHitEnemyBodyTask.value.Execute(agent.skillDriverOwner, blackboard);
                    }
                    break;
                case HitResType.PlayerHitEnemyBlock:
                    if (!PlayerHitEnemyBodyTask.isNoneOrNull)
                    {
                        PlayerHitEnemyBodyTask.value.Execute(agent.skillDriverOwner, blackboard);
                    }
                    break;
            }
        }
        EndAction(true);
    }
}
