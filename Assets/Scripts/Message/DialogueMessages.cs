using System;
using System.Collections.Generic;

public struct NormalDialogueMessage : IMessage
{
    public uint npcId;
    public string key;
    public Action CallBack;
}

public struct ChoiceDialogueMessage : IMessage
{
    public uint npcId;
    public string key;
    public Action<int> CallBack;
    public List<string> choicesKeys;
}

public struct NormalDialogueFinishMessage : IMessage { }