using Cysharp.Threading.Tasks;
using GameBase.Log;
using GameScene.FlowNode.Base;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Name("发送通用异步消息到指定蓝图")]
[Category("对话")]
[ParadoxNotion.Design.Icon("UpwardsArrow")]
[Color("00F5F5")]
public class DialogueSendGameFlowAsyncMsgNode : DTNode
{
    public BBParameter<uint> gamePlayId;
    public BBParameter<bool> isItsSpawnedItem = true;
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
        var root = World.Ins.GetRootByEntityId(gamePlayId.value);
        if(root != null)
        {
            if (gamePlayId.value == 0)
            {
                await root.SendGamePlayAsyncMsg(new OnCommonGameFlowAsyncMsg
                {
                    boolValue = boolValue.value,
                    intValue = intValue.value,
                    stringValue = stringValue.value,
                    floatValue = floatValue.value
                });
            }
            else
            {
                if(isItsSpawnedItem.value)
                {
                    var isp = root.GetAGamePlayEntity<GamePlayItemSpawnPoint>(gamePlayId.value);
                    if(isp != null)
                    {
                        var item = isp.spawnedEntity;
                        if(item != null)
                        {
                            await item.SendGamePlayAsyncMsg(new OnCommonGameFlowAsyncMsg
                            {
                                boolValue = boolValue.value,
                                intValue = intValue.value,
                                stringValue = stringValue.value,
                                floatValue = floatValue.value
                            });
                        }
                        else
                        {
                            LogUtils.Error($"没找到gamePlayId为 {gamePlayId.value} 的GamePlayItemSpawnPoint的spawnedEntity");
                        }
                    }
                    else
                    {
                        LogUtils.Error($"没找到gamePlayId为 {gamePlayId.value} 的GamePlayItemSpawnPoint");
                    }
                }
                else
                {
                    var item = root.GetAGamePlayEntity<GamePlayItem>(gamePlayId.value);
                    if (item != null)
                    {
                        await item.SendGamePlayAsyncMsg(new OnCommonGameFlowAsyncMsg
                        {
                            boolValue = boolValue.value,
                            intValue = intValue.value,
                            stringValue = stringValue.value,
                            floatValue = floatValue.value
                        });
                    }
                    else
                    {
                        LogUtils.Error($"没找到gamePlayId为 {gamePlayId.value} 的GamePlayItem");
                    }
                }
            }
        }
        status = Status.Success;
        if (outConnections.Count <= 0)
        {
            Message.Send(new NormalDialogueFinishMessage());
        }
        DLGTree.Continue();
    }

#if UNITY_EDITOR
    protected override void OnNodeGUI()
    {
        if(gamePlayId.value == 0)
        {
            GUILayout.Label($"<color=#00ffff><b>相当前关卡发送</b></color>");
        }
        else
        {
            if(isItsSpawnedItem.value)
            {
                GUILayout.Label($"<color=#00ffff><b>相当前关卡内gamePlayId为 {gamePlayId.value} 的GamePlayItemSpawnPoint对应生成的GamePlayItem发送</b></color>");
            }
            else
            {
                GUILayout.Label($"<color=#00ffff><b>相当前关卡内gamePlayId为 {gamePlayId.value} 的GamePlayItem发送</b></color>");
            }
        }
        GUILayout.Label($"<color=#00ff00><b>bool : </b></color> <color=#ff0000>{boolValue.value}</color>");
        GUILayout.Label($"<color=#00ff00><b>int : </b></color> <color=#ff0000>{intValue.value}</color>");
        GUILayout.Label($"<color=#00ff00><b>string : </b></color> <color=#ff0000>{stringValue.value}</color>");
        GUILayout.Label($"<color=#00ff00><b>float : </b></color> <color=#ff0000>{floatValue.value}</color>");
    }

    public override string name
    {
        get { return "发送通用异步消息到指定蓝图"; }
        set { }
    }
#endif
}
