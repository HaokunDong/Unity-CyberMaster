using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("GamePlay")]
public class AddACooldownAndSaveInBBAction : ActionTask
{
    public BBParameter<float> CDTime = 1f;
    public UpdateMode updateMode = UpdateMode.FixedUpdate;
    public string cdName;

    protected override void OnExecute()
    {
        base.OnExecute();
        if (agent != null && blackboard != null)
        {
            var gpe = agent.gameObject.GetComponent<GamePlayEntity>();
            if (gpe != null)
            {
                var gp = ManagerCenter.Ins.CooldownMgr.GetOrCreateGroup(gpe.GamePlayId, UpdateMode.FixedUpdate);
                var cd = gp.Set(cdName, CDTime.value);
                blackboard.SetVariableValue(cdName, cd);
            }
        }
        EndAction(true);
    }
}
