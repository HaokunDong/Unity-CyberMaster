using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Name("检查玩家是否在地面")]
public class GamePlayerIsGroundConditionTask : ConditionTask<GamePlayPlayer>
{
    protected override string info => $"玩家是否在地面";

    protected override bool OnCheck()
    {
        if (agent == null)
        {
            return false;
        }

        return agent.IsGrounded();
    }
}
