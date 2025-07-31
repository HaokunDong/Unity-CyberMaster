using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using System;
using UnityEngine;

[Flags]
public enum InputToDoFlags
{
    [InspectorName("无")]
    None = 0,

    [InspectorName("取消技能")]
    CancelSkill = 1 << 0, // 00001

    [InspectorName("立刻变招")]
    ChangeSkill = 1 << 1, // 00010

    [InspectorName("之后连招")]
    NextSkill = 1 << 2, // 00100

    [InspectorName("当前技能跳帧")]
    JumpSkillFrame = 1 << 3, // 01000

    [InspectorName("跳过一定尾帧 之后连招")]
    NextSkillWithTailCut = 1 << 4, // 10000
}

public static class InputToDoFlagsExtensions
{
    public static bool Has(this InputToDoFlags current, InputToDoFlags flag)
    {
        return (current & flag) == flag;
    }

    public static bool Any(this InputToDoFlags current, InputToDoFlags flags)
    {
        return (current & flags) != 0;
    }
}

public class SkillPlayerInputClip : SkillClipBase
{
    [NonSerialized]
    public SkillConfig config;
    public InputToDoFlags inputToDoFlags = InputToDoFlags.None;
    [InspectorName("当前技能最多播放到第几帧")]
    public int NextSkillTailCutFrame = -1;

    private bool CheckAllMatch(ref CommandInputState[] cs, string str, bool cleanInputWhenMatch)
    {
        if (cs.Length > 0 && !str.IsNullOrWhitespace())
        {
            bool allMatch = true;
            for (int i = 0; i < cs.Length; i++)
            {
                if (!InputButtonState.StaticIsMatchAny(cs[i]))
                {
                    allMatch = false;
                    break;
                }
            }
            if(allMatch)
            {
                for (int i = 0; i < cs.Length; i++)
                {
                    InputButtonState.GetButtonState(cs[i].CMD)?.ClearFlagThisFrame();
                }
            }
            return allMatch;
        }
        return false;
    }

    public override void OnClipUpdate(int frame)
    {
        base.OnClipUpdate(frame);
        if(inputToDoFlags.Has(InputToDoFlags.CancelSkill))
        {
            if(config.cancelCommandInputState != null && config.cancelCommandInputState.CMD != InputCommand.None)
            {
                if(InputButtonState.StaticIsMatchAny(config.cancelCommandInputState))
                {
                    config.skillDriver.CancelSkill(true, true).Forget();
                    return;
                }
            }
        }

        if (inputToDoFlags.Has(InputToDoFlags.JumpSkillFrame))
        {
            if (config.SkillJumpFrameCommandInputStateDict.Count > 0)
            {
                foreach (var kvp in config.SkillJumpFrameCommandInputStateDict)
                {
                    var cs = kvp.Key;
                    if (CheckAllMatch(ref cs, "Jump", true))
                    {
                        config.skillDriver.JumpToFrame(kvp.Value);
                        return;
                    }
                }
            }
        }

        if (inputToDoFlags.Has(InputToDoFlags.ChangeSkill))
        {
            if(config.ChangeSkillCommandInputStateDict.Count > 0)
            {
                foreach (var kvp in config.ChangeSkillCommandInputStateDict)
                {
                    var cs = kvp.Key;
                    if(CheckAllMatch(ref cs, kvp.Value, true))
                    {
                        config.skillDriver.ChangeSkillAsync(kvp.Value).Forget();
                        return;
                    }
                }
            }
        }

        if (inputToDoFlags.Has(InputToDoFlags.NextSkillWithTailCut) && config.skillDriver.BufferedSkillName.IsNullOrWhitespace())
        {
            if (config.AfterSkillCommandInputStateDict.Count > 0)
            {
                foreach (var kvp in config.AfterSkillCommandInputStateDict)
                {
                    var cs = kvp.Key;
                    if (CheckAllMatch(ref cs, kvp.Value, true))
                    {
                        config.skillDriver.BufferNextSkill(kvp.Value, NextSkillTailCutFrame);
                        return;
                    }
                }
            }
        }

        if (inputToDoFlags.Has(InputToDoFlags.NextSkill) && config.skillDriver.BufferedSkillName.IsNullOrWhitespace())
        {
            if (config.AfterSkillCommandInputStateDict.Count > 0)
            {
                foreach (var kvp in config.AfterSkillCommandInputStateDict)
                {
                    var cs = kvp.Key;
                    if (CheckAllMatch(ref cs, kvp.Value, true))
                    {
                        config.skillDriver.BufferNextSkill(kvp.Value, -1);
                        return;
                    }
                }
            }
        }
        
    }
}

[Serializable]
public class SkillPlayerInputTrack : BaseSkillTrack<SkillPlayerInputClip>
{
    public override void Init(SkillConfig config, object o)
    {
        base.Init(config, o);

        foreach (var clip in skillClipDict.Values)
        {
            clip.config = config;
        }
    }
}