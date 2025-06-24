using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using FlowCanvas;
using GameBase.Log;
using GameScene.FlowNode.Base;
using ParadoxNotion.Design;

namespace GameScene.FlowNode.GamePlayNode
{
    [Name("加载Prefab并播放Timeline")]
    [Category("常用")]
    public class GamePlayLoadPrefabAndPlayTimelineNode : BaseFlowActionAsync
    {
        public ValueInput<string> PrefabPath;
        public bool destroyAfterPlay = true;

        protected override void RegisterPorts()
        {
            base.RegisterPorts();
            PrefabPath = AddValueInput<string>("Prefab 路径");
        }

        protected override UniTask InvokeFunction(Flow flow)
        {
            if (GamePlayRoot.Current == null)
            {
                LogUtils.Error($"GamePlayLoadPrefabAndPlayTimelineNode GamePlayRoot.Current 为 null Path:{GetController().gameObject.GetPath()}");
                return UniTask.CompletedTask;
            }

            var path = PrefabPath.value;
            if (string.IsNullOrEmpty(path))
            {
                LogUtils.Error($"GamePlayLoadPrefabAndPlayTimelineNode 路径为空 Path:{GetController().gameObject.GetPath()}");
                return UniTask.CompletedTask;
            }

            // 调用你自定义的方法：按路径加载Prefab并播放Timeline
            return ManagerCenter.Ins.TimeLineMgr.LoadPrefabAndPlayTimelineAsync(path, destroyAfterPlay);
        }
    }
}
