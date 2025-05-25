using FlowCanvas.Nodes;
using GameScene.FlowNode.Base;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace GameScene.FlowNode
{
    [Name("GamePlay专用SubFlow")]
    [DropReferenceType(typeof(GamePlayFlowGraph))]
    [Icon("FS")]
    public class GamePlaySubFlow : FlowNestedBase<GamePlayFlowGraph>, IBaseFlowNode
    {
        [Name("AutoStart自动Start Stop")]
        public bool autoStart = true;
        public override void OnGraphStarted()
        {
            if (autoStart)
            {
                this.TryStartSubGraph(graphAgent, null);
            }
        }

        public override void OnGraphStoped()
        {
            if (autoStart)
            {
                subGraph.Stop();
            }
        }

        public void OnAfterInit()
        {
            if (subGraph && subGraph.allNodes != null)
            {
                for (var i = 0; i < subGraph.allNodes.Count; i++)
                {
                    if (subGraph.allNodes[i] is IBaseFlowNode node)
                    {
                        node.OnAfterInit();
                    }
                }
            }
        }
    }
}