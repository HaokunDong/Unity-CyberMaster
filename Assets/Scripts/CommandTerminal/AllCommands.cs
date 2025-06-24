using GameBase.Log;
using UnityEngine;
namespace CommandTerminal
{
    public class AllCommands
    {
        [RegisterCommand(Name = "loc", Help = "", MinArgCount = 1, MaxArgCount = 1)]
        public static void CommandLocalizationTest(CommandArg[] args)
        {
            LogUtils.Warning(args[0].String, LogChannel.Common, Color.cyan);
        }
    }
}
