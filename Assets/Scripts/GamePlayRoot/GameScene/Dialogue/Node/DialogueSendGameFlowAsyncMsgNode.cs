using Cysharp.Threading.Tasks;
using GameScene.FlowNode.Base;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Name("发送通用异步消息到关卡蓝图")]
[Category("对话")]
[ParadoxNotion.Design.Icon("UpwardsArrow")]
[Color("00F5F5")]
public class DialogueSendGameFlowAsyncMsgNode : DTNode
{
    public BBParameter<bool> boolValue;
    public BBParameter<int> intValue;
    public BBParameter<string> stringValue;
    public BBParameter<float> floatValue;

    protected override Status OnExecute(Component agent, IBlackboard blackboard)
    {
        SendAsyncMessage().Forget();
        return Status.Running;
    }

    private async UniTask SendAsyncMessage()
    {
        if(GamePlayRoot.Current != null)
        {
            await GamePlayRoot.Current.SendGamePlayAsyncMsg(new OnCommonGameFlowAsyncMsg 
            {
                boolValue = boolValue.value,
                intValue = intValue.value,
                stringValue = stringValue.value,
                floatValue = floatValue.value
            });
        }
        status = Status.Success;
        DLGTree.Continue();
    }

#if UNITY_EDITOR

    protected override void OnNodeGUI()
    {
        GUILayout.Label($"<color=#00ff00><b>bool : </b></color> <color=#ff0000>{boolValue.value}</color>");
        GUILayout.Label($"<color=#00ff00><b>int : </b></color> <color=#ff0000>{intValue.value}</color>");
        GUILayout.Label($"<color=#00ff00><b>string : </b></color> <color=#ff0000>{stringValue.value}</color>");
        GUILayout.Label($"<color=#00ff00><b>float : </b></color> <color=#ff0000>{floatValue.value}</color>");
    }

    public override string name
    {
        get { return "发送通用异步消息到关卡蓝图"; }
        set { }
    }
#endif
}
