using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputCommand
{
    None = 0,
    Attack = 1,
    Block = 2,
    Jump = 3,
    Dodge = 4,
    Skill1 = 5,
    Skill2 = 6,
}

[Flags]
public enum InputButtonFlags
{
    [InspectorName("无")]
    None = 0,

    [InspectorName("按下")]
    IsPressed = 1 << 0, // 00001

    [InspectorName("刚按下")]
    JustPressed = 1 << 1, // 00010

    [InspectorName("刚松开")]
    JustReleased = 1 << 2, // 00100

    [InspectorName("长按中")]
    LongPressed = 1 << 3, // 01000

    [InspectorName("双击")]
    DoubleTapped = 1 << 4, // 10000

    [InspectorName("多击")]
    MultiTapped = 1 << 5, // 100000
}

public static class InputButtonFlagsExtensions
{
    public static bool Has(this InputButtonFlags current, InputButtonFlags flag)
    {
        return (current & flag) == flag;
    }

    public static bool Any(this InputButtonFlags current, InputButtonFlags flags)
    {
        return (current & flags) != 0;
    }
}

[Serializable]
public class CommandInputState : IEquatable<CommandInputState>
{
    public InputCommand CMD;
    public InputButtonFlags InputState;

    public bool Equals(CommandInputState other)
    {
        if (other == null) return false;
        return this.CMD == other.CMD && this.InputState == other.InputState;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as CommandInputState);
    }

    public override int GetHashCode()
    {
        return ((int)CMD * 397) ^ (int)InputState;
    }

    public static bool operator ==(CommandInputState a, CommandInputState b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
        return a.Equals(b);
    }

    public static bool operator !=(CommandInputState a, CommandInputState b)
    {
        return !(a == b);
    }
}

public class InputButtonState
{
    private static Dictionary<InputCommand, InputButtonState> ButtonStates;

    public InputButtonFlags ButtonStateFlag { get; private set; }

    public int TapCount { get; private set; }

    private InputAction inputAction;
    private InputCommand cmd;
    private float holdTime = 0f;
    private float lastTapTime = -1f;

    // 可配置参数
    private readonly float doubleTapWindow;
    private readonly float longPressThreshold;
    private readonly float multiTapWindow;
    private readonly int multiTapCount;
    private bool oldPressed = false;

    private float justPressedCacheTime = 0f;
    private float justReleasedCacheTime = 0f;
    private float cacheDuration = 0.1f; // 缓存有效时间，避免被技能系统错过

    public InputButtonState(
        InputAction inputAction,
        InputCommand cmd,
        float doubleTapWindow = 0.25f,
        float longPressThreshold = 0.5f,
        float multiTapWindow = 0.4f,
        int multiTapCount = 3)
    {
        ButtonStates ??= new();
        ButtonStates[cmd] = this;

        this.inputAction = inputAction;
        this.cmd = cmd;
        this.doubleTapWindow = doubleTapWindow;
        this.longPressThreshold = longPressThreshold;
        this.multiTapWindow = multiTapWindow;
        this.multiTapCount = multiTapCount;
    }

    public static InputButtonState GetButtonState(InputCommand cmd)
    {
        InputButtonState state = null;
        ButtonStates?.TryGetValue(cmd, out state);
        return state;
    }

    public void ClearFlagThisFrame()
    {
        ButtonStateFlag = InputButtonFlags.None;
        justPressedCacheTime = 0;
        justReleasedCacheTime = 0;
    }

    public void Update(float deltaTime)
    {
        var isCurrentlyPressed = inputAction != null && inputAction.IsPressed();
        ButtonStateFlag = InputButtonFlags.None;

        justPressedCacheTime = Mathf.Max(0f, justPressedCacheTime - deltaTime);
        justReleasedCacheTime = Mathf.Max(0f, justReleasedCacheTime - deltaTime);

        // 检测按下
        if (isCurrentlyPressed && !oldPressed)
        {
            ButtonStateFlag |= InputButtonFlags.JustPressed;
            justPressedCacheTime = cacheDuration;
            justReleasedCacheTime = 0;

            float now = Time.time;

            // 判断双击
            if (now - lastTapTime <= doubleTapWindow)
            {
                TapCount++;
                if (TapCount == 2)
                    ButtonStateFlag |= InputButtonFlags.DoubleTapped;
            }
            else
            {
                TapCount = 1;
            }

            // 判断 n 连击
            if (now - lastTapTime <= multiTapWindow)
            {
                if (TapCount >= multiTapCount)
                {
                    ButtonStateFlag |= InputButtonFlags.MultiTapped;
                    TapCount = 0; // 重置避免误判
                }
            }
            else
            {
                TapCount = 1;
            }

            lastTapTime = now;
        }

        // 检测松开
        if (!isCurrentlyPressed && oldPressed)
        {
            ButtonStateFlag |= InputButtonFlags.JustReleased;
            justReleasedCacheTime = cacheDuration;
            justPressedCacheTime = 0;
            holdTime = 0f;
            ButtonStateFlag &= ~InputButtonFlags.LongPressed;
        }

        // 持续按住时间
        if (isCurrentlyPressed)
        {
            holdTime += deltaTime;
            if (!ButtonStateFlag.HasFlag(InputButtonFlags.LongPressed) && holdTime >= longPressThreshold)
                ButtonStateFlag |= InputButtonFlags.LongPressed;
        }

        if(isCurrentlyPressed)
        {
            ButtonStateFlag |= InputButtonFlags.IsPressed;
        }
        else
        {
            ButtonStateFlag &= ~InputButtonFlags.IsPressed;
        }

        oldPressed = isCurrentlyPressed;
    }

    public bool IsMatchAny(CommandInputState CMI)
    {
        if (CMI == null || CMI.CMD != cmd) return false;

        InputButtonFlags target = CMI.InputState;
        InputButtonFlags current = ButtonStateFlag;

        if (target.Has(InputButtonFlags.JustPressed) && justPressedCacheTime > 0)
            return true;
        if (target.Has(InputButtonFlags.JustReleased) && justReleasedCacheTime > 0)
            return true;

        return current.Any(target & ~(InputButtonFlags.JustPressed | InputButtonFlags.JustReleased));
    }

    public static bool StaticIsMatchAny(CommandInputState CMI)
    {
        if(CMI == null) return false;
        if (ButtonStates.TryGetValue(CMI.CMD, out var state))
        {
            return state.IsMatchAny(CMI);
        }
        return false;
    }
}
