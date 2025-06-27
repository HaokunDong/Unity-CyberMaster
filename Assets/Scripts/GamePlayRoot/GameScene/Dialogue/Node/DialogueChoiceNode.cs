using Cysharp.Text;
using Everlasting.Config;
using Localization;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using ParadoxNotion.Design;
using System;
using System.Collections.Generic;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[Serializable]
public class LocalizationChoice
{
    public string key;

#if UNITY_EDITOR
    [NonSerialized]
    public bool isUnfolded = true;
    [NonSerialized]
    public Vector2 scrollPos = Vector2.zero;
    [NonSerialized]
    public string oldKey;
    [NonSerialized]
    public List<string> aks = new List<string>();
#endif

    public LocalizationChoice() { }
    public LocalizationChoice(string key)
    {
        this.key = key;
    }
}

[Name("对话选项")]
[Category("对话")]
[ParadoxNotion.Design.Icon("List")]
[Color("00F500")]
public class DialogueChoiceNode : DTNode
{
    private static List<string> cks = new List<string>();

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
        cks.Clear();
        foreach (var choice in choices)
        {
            cks.Add(choice.key);
        }

        Message.Send(new ChoiceDialogueMessage
        {
            npcId = NpcId < 0 ? (uint)agent.GetComponent<GamePlayEntity>()?.TableId : (uint)NpcId,
            key = Key,
            CallBack = OnStatementFinish,
            choicesKeys = cks,
        });
        return Status.Running;
    }

    private void OnStatementFinish(int i)
    {
        status = Status.Success;
        if (outConnections.Count <= i)
        {
            Message.Send(new NormalDialogueFinishMessage());
        }
        DLGTree.Continue(i);
    }

    [SerializeField]
    private List<LocalizationChoice> choices = new List<LocalizationChoice>();

    public override int maxOutConnections { get { return choices.Count; } }

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

        if (GUILayout.Button("添加选项"))
        {
            choices.Add(new LocalizationChoice());
        }

        if (choices.Count == 0)
        {
            return;
        }

        EditorUtils.ReorderableList(choices, (i, picked) =>
        {
            var choice = choices[i];
            GUILayout.BeginHorizontal("box");

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                choices.RemoveAt(i);
                if (i < outConnections.Count)
                {
                    graph.RemoveConnection(outConnections[i]);
                }
            }

            var text = ZString.Format("{0} {1}", choice.isUnfolded ? "▼ " : "► ", LocalizationManager.setting.Get(choice.key));
            if (GUILayout.Button(text, (GUIStyle)"label", GUILayout.Width(0), GUILayout.ExpandWidth(true)))
            {
                choice.isUnfolded = !choice.isUnfolded;
            }

            GUILayout.EndHorizontal();

            if (choice.isUnfolded)
            {
                DoChoiceGUI(choice, i);
            }
        });
    }

    private void DoChoiceGUI(LocalizationChoice choice, int i)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical("box");

        choice.key = EditorGUILayout.TextField($"选项 {i + 1} 本地化Key: ", choice.key);
        EditorGUILayout.LabelField("文本预览: ", LocalizationManager.setting.Get(choice.key));

        if (!string.IsNullOrEmpty(choice.key))
        {
            if (choice.oldKey != choice.key)
            {
                choice.aks.Clear();
                choice.oldKey = choice.key;
                var dict = LocalizationManager.GetLanguagesContains(choice.oldKey);
                if (dict != null && dict.Keys.Count > 0)
                {
                    foreach (var dk in dict.Keys)
                    {
                        choice.aks.Add(dk);
                    }
                }
            }
        }
        else
        {
            choice.oldKey = String.Empty;
            choice.aks.Clear();
        }

        var wLen = math.min(choice.aks.Count * 23, 400);
        choice.scrollPos = GUILayout.BeginScrollView(choice.scrollPos, GUILayout.Width(380), GUILayout.Height(wLen));
        if (choice.aks != null && choice.aks.Count > 0)
        {
            foreach (var ak in choice.aks)
            {
                var lab = "K: " + ak + " V: " + LocalizationManager.setting.Get(ak);
                if (GUILayout.Button(lab))
                {
                    choice.oldKey = ak;
                    choice.key = ak;
                    choice.aks.Clear();
                    GUI.FocusControl(null);
                    break;
                }
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
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
        for (int i = 0; i < choices.Count; i++)
        {
            GUILayout.Label($"<color=#00ffff><b>选项 {i + 1} : </b></color> <color=#ff0000>{LocalizationManager.setting.Get(choices[i].key)}</color>");
        }
    }

    public override string name
    {
        get { return "选项对话"; }
        set { }
    }

    public override void OnConnectionInspectorGUI(int i)
    {
        DoChoiceGUI(choices[i], i);
    }

    public override string GetConnectionInfo(int i)
    {
        if (i >= choices.Count)
        {
            return "NOT SET";
        }
        var text = ZString.Format("'{0}'", LocalizationManager.setting.Get(choices[i].key));
        return text;
    }
#endif
}
