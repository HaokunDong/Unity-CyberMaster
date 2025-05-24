using System;

namespace GameBase.Log
{
    [Flags]
    public enum LogChannel
    {
        Common = 0x0001,
        Battle = 0x0002,
        UI = 0x0004,
        Load = 0x0008,
        GamePlay = 0x0010,
        Effect = 0x0020,
        RenderPipeline = 0x0040,
        Audio = 0x0080,
        HotUpdate = 0x0100,
        
        Message = 0x8000,
        AllNoMessage = 0xffff - Message,
    }
}