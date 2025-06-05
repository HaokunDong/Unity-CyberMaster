using Cysharp.Text;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

partial class SkillEditorInspector
{
    #region 动画轨道
    private Label clipFrameLabel;
    private Toggle rootMotionToggle;
    private Label isLoopLable;
    private IntegerField durationField;
    private FloatField transitionTimeField;

    private void DrawTrackItem(AnimationTrackItem item)
    {
        //动画资源
        ObjectField animationClipAssetField = new ObjectField("动画资源");
        animationClipAssetField.objectType = typeof(AnimationClip);
        animationClipAssetField.value = item.Clip.AnimationClip;
        animationClipAssetField.RegisterValueChangedCallback(AnimationClipValueChangedCallback);
        root.Add(animationClipAssetField);

        //根运动
        rootMotionToggle = new Toggle("应用根运动");
        rootMotionToggle.value = item.Clip.ApplyRootMotion;
        rootMotionToggle.RegisterValueChangedCallback(rootMotionToggleValueChanged);
        root.Add(rootMotionToggle);

        //过渡时间
        transitionTimeField = new FloatField("过渡时间");
        transitionTimeField.value = item.Clip.TransitionTime;
        transitionTimeField.RegisterValueChangedCallback(TransitionTimeFieldValueChangedCallback);
        root.Add(transitionTimeField);

        //动画相关的信息
        int clipFrameCount = (int)(item.Clip.AnimationClip.length * item.Clip.AnimationClip.frameRate);
        clipFrameLabel = new Label(ZString.Concat("片段帧数 ", clipFrameCount));
        root.Add(clipFrameLabel);

        isLoopLable = new Label("循环动画：" + item.Clip.AnimationClip.isLooping);
        root.Add(isLoopLable);
    }

    private void AnimationClipValueChangedCallback(ChangeEvent<UnityEngine.Object> evt)
    {
        AnimationClip clip = evt.newValue as AnimationClip;

        //修改自身显示效果
        clipFrameLabel.text = "动画资源长度：" + ((int)(clip.length * clip.frameRate));
        isLoopLable.text = "循环动画：" + clip.isLooping;

        //保存到配置
        (currentTrackItem as AnimationTrackItem).Clip.AnimationClip = clip;
        SkillEditorWindows.Instance.SaveConfig();
        currentTrackItem.ResetView();
    }

    private void rootMotionToggleValueChanged(ChangeEvent<bool> evt)
    {
        (currentTrackItem as AnimationTrackItem).Clip.ApplyRootMotion = evt.newValue;
        SkillEditorWindows.Instance.SaveConfig();
    }

    private void TransitionTimeFieldValueChangedCallback(ChangeEvent<float> evt)
    {
        (currentTrackItem as AnimationTrackItem).Clip.TransitionTime = evt.newValue;
        SkillEditorWindows.Instance.SaveConfig();
        currentTrack.ResetView();
    }
    #endregion
}
