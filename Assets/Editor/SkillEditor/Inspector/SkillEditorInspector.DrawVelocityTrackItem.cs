using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

partial class SkillEditorInspector
{
    private void DrawVelocityTrackItem(VelocityTrackItem item)
    {
        trackItemFrameIndex = item.FrameIndex;
        root.Clear();

        //轨道长度
        durationField = new IntegerField("片段帧数");
        durationField.value = item.Clip.DurationFrame;
        durationField.RegisterValueChangedCallback(TrackDurationFieldValueChangedCallback<VelocityTrack, VelocityTrackItem, SkillVelocityClip>);
        root.Add(durationField);

        var facePlayerFirstFrame = new Toggle("片段第一帧面向玩家");
        facePlayerFirstFrame.value = item.Clip.FacePlayerFirstFrame;
        facePlayerFirstFrame.RegisterValueChangedCallback((ChangeEvent<bool> evt) => 
        {
            item.Clip.FacePlayerFirstFrame = evt.newValue;
            SkillEditorWindows.Instance.SaveConfig();
            currentTrack.ResetView();
        });
        root.Add(facePlayerFirstFrame);

        var facePlayerEveryFrame = new Toggle("片段每一帧面向玩家");
        facePlayerEveryFrame.value = item.Clip.FacePlayerEveryFrame;
        facePlayerEveryFrame.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
        {
            item.Clip.FacePlayerEveryFrame = evt.newValue;
            SkillEditorWindows.Instance.SaveConfig();
            currentTrack.ResetView();
        });
        root.Add(facePlayerEveryFrame);

        var useCurrentFacing = new Toggle("速度X相对当前朝向");
        useCurrentFacing.value = item.Clip.UseCurrentFacing;
        useCurrentFacing.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
        {
            item.Clip.UseCurrentFacing = evt.newValue;
            SkillEditorWindows.Instance.SaveConfig();
            currentTrack.ResetView();
        });
        root.Add(useCurrentFacing);

        var velocity = new Vector2Field("速度");
        velocity.value = item.Clip.velocity;
        velocity.RegisterValueChangedCallback((ChangeEvent<Vector2> evt) =>
        {
            item.Clip.velocity = evt.newValue;
            SkillEditorWindows.Instance.SaveConfig();
            currentTrack.ResetView();
        });
        root.Add(velocity);

        //删除
        Button deleteButton = new Button(DeleteButtonClick);
        deleteButton.text = "删除";
        deleteButton.style.backgroundColor = new Color(1, 0, 0, 0.5f);
        root.Add(deleteButton);
    }
}
