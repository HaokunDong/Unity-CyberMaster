using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("GamePlay")]
public class SwitchSkillDiverHitResTypeAction : ActionTask<ISkillDriverUnit>
{
    [Header("敌人直接命中玩家时")]
    public BBParameter<ActionTask> EnemyHitPlayerBodyTask;
    [Header("敌人命中玩家格挡时")]
    public BBParameter<ActionTask> EnemyHitPlayerBlockTask;
    [Header("玩家敌人拼刀时")]
    public BBParameter<ActionTask> PlayerEnemyBladeFightTask;
    [Header("玩家直接命中敌人时")]
    public BBParameter<ActionTask> PlayerHitEnemyBodyTask;
    [Header("玩家命中敌人格挡时")]
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
