using System;

public struct NormalDialogueMessage : IMessage
{
    public uint npcId;
    public string key;
    public Action CallBack;
}

public struct NormalDialogueFinishMessage : IMessage { }