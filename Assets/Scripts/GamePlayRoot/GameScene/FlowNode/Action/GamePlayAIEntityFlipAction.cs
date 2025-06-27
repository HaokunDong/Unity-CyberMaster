using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("GamePlay")]
public class GamePlayAIEntityFlipAction : ActionTask<GamePlayAIEntity>
{
    protected override void OnExecute()
    {
        base.OnExecute();
        agent.Flip();
        EndAction(true);
    }
}
