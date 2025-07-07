using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Everlasting.Extend;
using Localization;
using Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager
{
    public bool IsInDialogue { get; private set; }
    private MessageCollection m_messages;
    private List<string> choices = new List<string>();

    public void Init()
    {
        m_messages ??= new MessageCollection();
        m_messages.AddListener<NormalDialogueMessage>(OnNormalDialogueMessage);
        m_messages.AddListener<NormalDialogueFinishMessage>(OnNormalDialogueFinishMessage);
        m_messages.AddListener<ChoiceDialogueMessage>(OnChoiceDialogueMessage);
        IsInDialogue = false;
    }

    private void OnChoiceDialogueMessage(in ChoiceDialogueMessage message)
    {
        if (!IsInDialogue)
        {
            IsInDialogue = true;
        }
        var hp = "";
        var name = "";
        if (message.npcId == 0)
        {
            hp = "Heads/0";
            name = "me";
        }
        else
        {
            var data = NPCTable.GetTableData(message.npcId);
            if (data != null)
            {
                hp = data.DialogueHeadPath;
                name = data.NPCName;
            }
        }
        SendToUI(name, hp, message.key, null, message.choicesKeys, message.CallBack).Forget();
    }

    private void OnNormalDialogueMessage(in NormalDialogueMessage message)
    {
        if(!IsInDialogue)
        {
            IsInDialogue = true;
        }
        var hp = "";
        var name = "";
        if(message.npcId == 0)
        {
            hp = "Heads/0";
            name = "me";
        }
        else
        {
            var data = NPCTable.GetTableData(message.npcId);
            if (data != null)
            {
                hp = data.DialogueHeadPath;
                name = data.NPCName;
            }
        }
        SendToUI(name, hp, message.key, message.CallBack).Forget();
    }

    private async UniTask SendToUI(string name, string headPath, string key, Action action = null, List<string> options = null, Action<int> cAction = null)
    {
        choices.Clear();
        Sprite sprite = null;
        if (!headPath.IsNullOrEmpty())
        {
            sprite = await ResourceManager.LoadAssetAsync<Sprite>(headPath, ResType.UIPicture);
        }

        if(options != null)
        {
            for (int i = 0; i < options.Count; i++)
            {
                choices.Add(LocalizationManager.setting.Get(options[i]));
            }
        }
        
        TempDialoguePanel.Instance.Show(LocalizationManager.setting.Get(name), sprite, LocalizationManager.setting.Get(key), action, choices, cAction);
    }

    private void OnNormalDialogueFinishMessage(in NormalDialogueFinishMessage message)
    {
        IsInDialogue = false;
        TempDialoguePanel.Instance.Hide();
    }
}
