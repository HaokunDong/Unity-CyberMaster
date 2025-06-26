using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Everlasting.Extend;
using Localization;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager
{
    private MessageCollection m_messages;
    public bool IsInDialogue { get; private set; }

    public void Init()
    {
        m_messages ??= new MessageCollection();
        m_messages.AddListener<NormalDialogueMessage>(OnNormalDialogueMessage);
        m_messages.AddListener<NormalDialogueFinishMessage>(OnNormalDialogueFinishMessage);
        IsInDialogue = false;
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

    private async UniTask SendToUI(string name, string headPath, string key, Action action)
    {
        Sprite sprite = null;
        if (!headPath.IsNullOrEmpty())
        {
            sprite = await ResourceManager.LoadAssetAsync<Sprite>(headPath, ResType.UIPicture);
        }
        TempDialoguePanel.Instance.Show(LocalizationManager.setting.Get(name), sprite, LocalizationManager.setting.Get(key), action);
    }

    private void OnNormalDialogueFinishMessage(in NormalDialogueFinishMessage message)
    {
        IsInDialogue = false;
        TempDialoguePanel.Instance.Hide();
    }
}
