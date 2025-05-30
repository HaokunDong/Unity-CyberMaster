using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    private void DrawAnimationTrackItem(AnimationTrackItem item)
    {
        trackItemFrameIndex = item.FrameIndex;

        //动画资源
        ObjectField animationClipAssetField = new ObjectField("动画资源");
        animationClipAssetField.objectType = typeof(AnimationClip);
        animationClipAssetField.value = item.SkillAnimationClip.AnimationClip;
        animationClipAssetField.RegisterValueChangedCallback(AnimationClipValueChangedCallback);
        root.Add(animationClipAssetField);

        //根运动
        rootMotionToggle = new Toggle("应用根运动");
        rootMotionToggle.value = item.SkillAnimationClip.ApplyRootMotion;
        rootMotionToggle.RegisterValueChangedCallback(rootMotionToggleValueChanged);
        root.Add(rootMotionToggle);

        //轨道长度
        durationField = new IntegerField("片段帧数");
        durationField.value = item.SkillAnimationClip.DurationFrame;
        durationField.RegisterValueChangedCallback(DurtionFieldValueChangedCallback);
        root.Add(durationField);

        //过渡时间
        transitionTimeField = new FloatField("过渡时间");
        transitionTimeField.value = item.SkillAnimationClip.TransitionTime;
        transitionTimeField.RegisterValueChangedCallback(TransitionTimeFieldValueChangedCallback);
        root.Add(transitionTimeField);

        //动画相关的信息
        int clipFrameCount = (int)(item.SkillAnimationClip.AnimationClip.length * item.SkillAnimationClip.AnimationClip.frameRate);
        clipFrameLabel = new Label("动画资源长度：" + clipFrameCount);
        root.Add(clipFrameLabel);

        isLoopLable = new Label("循环动画：" + item.SkillAnimationClip.AnimationClip.isLooping);
        root.Add(isLoopLable);

        //删除
        Button deleteButton = new Button(DeleteButtonClick);
        deleteButton.text = "删除";
        deleteButton.style.backgroundColor = new Color(1, 0, 0, 0.5f);
        root.Add(deleteButton);
    }

    private void AnimationClipValueChangedCallback(ChangeEvent<UnityEngine.Object> evt)
    {
        AnimationClip clip = evt.newValue as AnimationClip;

        //修改自身显示效果
        clipFrameLabel.text = "动画资源长度：" + ((int)(clip.length * clip.frameRate));
        isLoopLable.text = "循环动画：" + clip.isLooping;

        //保存到配置
        (currentTrackItem as AnimationTrackItem).SkillAnimationClip.AnimationClip = clip;
        SkillEditorWindows.Instance.SaveConfig();
        currentTrackItem.ResetView();
    }

    private void rootMotionToggleValueChanged(ChangeEvent<bool> evt)
    {
        (currentTrackItem as AnimationTrackItem).SkillAnimationClip.ApplyRootMotion = evt.newValue;
        SkillEditorWindows.Instance.SaveConfig();
    }

    private void DurtionFieldValueChangedCallback(ChangeEvent<int> evt)
    {
        int value = evt.newValue;

        var at = currentTrack as AnimationTrack;
        //安全校验
        if (at.CheckFrameIndexOnDrag(at.AnimationData.skillAnimationClipDict, trackItemFrameIndex + value, trackItemFrameIndex, false))
        {
            //修改数据，刷新视图
            (currentTrackItem as AnimationTrackItem).SkillAnimationClip.DurationFrame = value;
            (currentTrackItem as AnimationTrackItem).CheckFrameCount();
            SkillEditorWindows.Instance.SaveConfig();//注意要最后保存，不然新旧数据会对不上
            currentTrackItem.ResetView();
        }
        else
        {
            durationField.value = evt.previousValue;
        }
    }

    private void TransitionTimeFieldValueChangedCallback(ChangeEvent<float> evt)
    {
        (currentTrackItem as AnimationTrackItem).SkillAnimationClip.TransitionTime = evt.newValue;
        SkillEditorWindows.Instance.SaveConfig();
        currentTrack.ResetView();
    }

    private void DeleteButtonClick()
    {
        currentTrack.DeleteTrackItem(trackItemFrameIndex); //此函数提供数据保存和刷新视图的逻辑
        Selection.activeObject = null;
        currentTrack.ResetView();
    }

    #endregion
}
