using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Name("是否在释放空中技能")]
public class ISkillDriverUnitPlayingAirSkillConditionTask : ConditionTask<ISkillDriverUnit>
{
    protected override string info => $"是否在释放空中技能";

    protected override bool OnCheck()
    {
        if (agent == null)
        {
            return false;
        }

        if(!agent.skillDriverImp.IsPlaying)
        {
            return false;
        }

        return agent.skillDriverImp.skillConfig.isAnAirSkill;
    }
}
