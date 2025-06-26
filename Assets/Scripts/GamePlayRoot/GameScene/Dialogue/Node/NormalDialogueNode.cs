using NodeCanvas.DialogueTrees;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using NodeCanvas.Framework;
using Localization;
using Cysharp.Text;
using ParadoxNotion.Design;
using Everlasting.Config;

[Name("普通对话")]
[Category("对话")]
[ParadoxNotion.Design.Icon("Action")]
[Color("FFA500")]
public class NormalDialogueNode : DTNode
{
    public int NpcId = -1;

    [SerializeField]
    private string m_key = string.Empty;
    public string Key
    {
        get
        {
            return m_key;
        }
#if UNITY_EDITOR
        set
        {
            if (m_key != value)
            {
                m_key = value;
                if (!string.IsNullOrEmpty(m_key))
                {
                    aks.Clear();
                    var dict = LocalizationManager.GetLanguagesContains(m_key);
                    if (dict != null && dict.Keys.Count > 0)
                    {
                        foreach (var dk in dict.Keys)
                        {
                            aks.Add(dk);
                        }
                    }
                }
                else
                {
                    aks.Clear();
                }
            }
        }
#endif
    }

    protected override Status OnExecute(Component agent, IBlackboard blackboard)
    {
        Message.Send(new NormalDialogueMessage 
        {
            npcId = NpcId < 0 ? (uint)agent.GetComponent<GamePlayEntity>()?.TableId : (uint)NpcId,
            key = Key,
            CallBack = OnStatementFinish,
        });
        return Status.Running;
    }

    void OnStatementFinish()
    {
        status = Status.Success;
        if(outConnections.Count <= 0)
        {
            Message.Send(new NormalDialogueFinishMessage());
        }
        DLGTree.Continue();
    }

#if UNITY_EDITOR
    [NonSerialized]
    public Vector2 scrollPos = Vector2.zero;
    [NonSerialized]
    public List<string> aks = new List<string>();

    protected override void OnNodeInspectorGUI()
    {
        //base.OnNodeInspectorGUI();
        NodeInspector();
    }

    protected virtual void NodeInspector()
    {
        NpcId = EditorGUILayout.IntField("NPC ID", NpcId);
        GUI.color = Color.green;
        if (NpcId < 0)
        {
            GUILayout.Label("说话者:当前对话树挂载对象");
        }
        else if (NpcId == 0)
        {
            GUILayout.Label("说话者:玩家自身");
        }
        else 
        {
            var data = NPCTable.GetTableData((uint)NpcId);
            if (data != null)
            {
                GUILayout.Label(ZString.Concat("说话者:", LocalizationManager.setting.Get(data.NPCName)));
            }
            else
            {
                GUI.color = Color.red;
                GUILayout.Label("说话者ID不对");
            }
        }
        GUI.color = Color.white;
        Key = EditorGUILayout.TextField("对话文本key", Key);
        GUI.color = Color.green;
        GUILayout.Label(ZString.Concat("文本: ", LocalizationManager.setting.Get(Key)));
        GUI.color = Color.white;
        if (aks.Count > 0)
        {
            var wLen = Math.Min(aks.Count * 23, 400);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(380), GUILayout.Height(wLen));
            foreach (var ak in aks)
            {
                var lab = "K: " + ak + " V: " + LocalizationManager.setting.Get(ak);
                if (GUILayout.Button(lab))
                {
                    Key = ak;
                    aks.Clear();
                    GUI.FocusControl(null);
                    break;
                }
            }
            GUILayout.EndScrollView();
        }
    }

    protected override void OnNodeGUI()
    {
        var name = "";
        if (NpcId < 0)
        {
            name = "当前对话树挂载对象";
        }
        else if (NpcId == 0)
        {
            name = "玩家";
        }
        else
        {
            var data = NPCTable.GetTableData((uint)NpcId);
            if (data != null)
            {
                name = LocalizationManager.setting.Get(data.NPCName);
            }
        }
        GUILayout.Label($"<color=#00ff00><b>{name} : </b></color> <color=#ff0000>{LocalizationManager.setting.Get(Key)}</color>");
    }

    public override string name
    {
        get { return "普通对话"; }
        set { }
    }
#endif
}
