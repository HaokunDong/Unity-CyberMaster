using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using FlowCanvas;
using GameBase.Log;
using GameScene.FlowNode.Base;
using ParadoxNotion.Design;

namespace GameScene.FlowNode.GamePlayNode
{
    [Name("生成")]
    [Category("常用")]
    public class GamePlaySpawnNode : BaseFlowActionAsync
    {
        public ValueInput<uint> GamePlayId;

        protected override void RegisterPorts()
        {
            base.RegisterPorts();
            GamePlayId = AddValueInput<uint>("Spawn Point Id");
        }

        protected override UniTask InvokeFunction(Flow flow)
        {
            var root = World.Ins.GetRootByEntityId(GamePlayId.value);
            if(root == null)
            {
                LogUtils.Error($"GamePlaySpawnNode GamePlayRoot.Current为null Path:{GetController().gameObject.GetPath()}");
                return UniTask.CompletedTask;
            }
            if(GamePlayId.value <= 0)
            {
                LogUtils.Error($"GamePlaySpawnNode 传入GamePlayId为0或负数 GamePlayId:{GamePlayId.value} Path:{GetController().gameObject.GetPath()}");
                return UniTask.CompletedTask;
            }
            return root.DoGamePlaySpawn(GamePlayId.value);
        }
    }
}
