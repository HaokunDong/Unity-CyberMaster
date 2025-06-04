#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Time Scale Controller", defaultDisplay: true)]
public class TimeScaleToolbarOverlay : ToolbarOverlay
{
    public TimeScaleToolbarOverlay() : base(
        TimeScaleButtonPause.ID,
        TimeScaleButtonHalf.ID,
        TimeScaleButtonNormal.ID,
        TimeScaleButtonDouble.ID,
        TimeScaleButtonFive.ID,
        TimeScaleInput.ID)
    { }
}

//
// 按钮定义，切换 Time.timeScale
//
[EditorToolbarElement(TimeScaleButtonPause.ID)]
public class TimeScaleButtonPause : EditorToolbarButton
{
    public const string ID = "TimeScale/Pause";
    public TimeScaleButtonPause()
    {
        text = "Pause";
        clicked += () => { if (EditorApplication.isPlaying) Time.timeScale = 0f; };
    }
}

[EditorToolbarElement(TimeScaleButtonHalf.ID)]
public class TimeScaleButtonHalf : EditorToolbarButton
{
    public const string ID = "TimeScale/0.5x";
    public TimeScaleButtonHalf()
    {
        text = "0.5x";
        clicked += () => { if (EditorApplication.isPlaying) Time.timeScale = 0.5f; };
    }
}

[EditorToolbarElement(TimeScaleButtonNormal.ID)]
public class TimeScaleButtonNormal : EditorToolbarButton
{
    public const string ID = "TimeScale/1x";
    public TimeScaleButtonNormal()
    {
        text = "1x";
        clicked += () => { if (EditorApplication.isPlaying) Time.timeScale = 1f; };
    }
}

[EditorToolbarElement(TimeScaleButtonDouble.ID)]
public class TimeScaleButtonDouble : EditorToolbarButton
{
    public const string ID = "TimeScale/2x";
    public TimeScaleButtonDouble()
    {
        text = "2x";
        clicked += () => { if (EditorApplication.isPlaying) Time.timeScale = 2f; };
    }
}

[EditorToolbarElement(TimeScaleButtonFive.ID)]
public class TimeScaleButtonFive : EditorToolbarButton
{
    public const string ID = "TimeScale/5x";
    public TimeScaleButtonFive()
    {
        text = "5x";
        clicked += () => { if (EditorApplication.isPlaying) Time.timeScale = 5f; };
    }
}

//
// 输入框定义，支持 0.1 ~ 10 范围输入，实时同步 Time.timeScale
//
[EditorToolbarElement(TimeScaleInput.ID)]
public class TimeScaleInput : VisualElement
{
    public const string ID = "TimeScale/Input";

    private readonly FloatField inputField;

    public TimeScaleInput()
    {
        inputField = new FloatField("TimeScale")
        {
            value = Time.timeScale
        };

        // 样式优化
        inputField.style.width = 160;
        inputField.style.fontSize = 14;
        inputField.style.unityFontStyleAndWeight = FontStyle.Bold;
        inputField.style.color = Color.white;
        var input = inputField.Q("unity-text-input");
        if (input != null)
        {
            input.style.color = Color.white;
            input.style.unityTextAlign = TextAnchor.MiddleRight;
        }

        Add(inputField);

        // 输入变化时应用 Time.timeScale
        inputField.RegisterValueChangedCallback(evt =>
        {
            if (!EditorApplication.isPlaying) return;

            float clamped = Mathf.Clamp(evt.newValue, 0.1f, 10f);
            Time.timeScale = clamped;
            inputField.SetValueWithoutNotify(clamped); // 防止重复触发
        });

        // 定时同步显示最新的 Time.timeScale
        EditorApplication.update += () =>
        {
            if (EditorApplication.isPlaying)
            {
                if (!Mathf.Approximately(inputField.value, Time.timeScale))
                {
                    inputField.SetValueWithoutNotify(Time.timeScale);
                }
            }
            else
            {
                inputField.SetValueWithoutNotify(1f);
            }
        };

        // 退出 Play Mode 自动重置
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Time.timeScale = 1f;
            }
        };
    }
}
#endif
