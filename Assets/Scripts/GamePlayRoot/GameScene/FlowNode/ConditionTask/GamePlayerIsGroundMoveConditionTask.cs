using NodeCanvas.Framework;
using UnityEngine;

public class GamePlayerIsGroundMoveConditionTask : ConditionTask<GamePlayPlayer>
{
    protected override string info => $"玩家是否在地面移动";

    protected override bool OnCheck()
    {
        if (agent == null)
        {
            return false;
        }

        if (!agent.IsGrounded())
        {
            return false;
        }

        Vector2 moveInput = agent.moveInput;
        return moveInput != Vector2.zero;
    }
}
