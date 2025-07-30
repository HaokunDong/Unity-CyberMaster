using UnityEngine.UIElements;

partial class SkillEditorInspector
{
    private JumpFrameTrackItem jumpFrameTrackItem;

    private void DrawTrackItem(JumpFrameTrackItem item)
    {
        jumpFrameTrackItem = item;
        var frameCountField = new IntegerField("片段帧数");
        frameCountField.value = item.Clip.DurationFrame;
        frameCountField.RegisterValueChangedCallback(TrackDurationFieldValueChangedCallback<JumpFrameTrack, JumpFrameTrackItem, SkillJumpFrameClip>);
        root.Add(frameCountField);

        var jump = new IntegerField("跳转到当前技能的第几帧");
        jump.value = item.Clip.Jump2Frame;
        jump.RegisterValueChangedCallback(evt =>
        {
            item.Clip.Jump2Frame = evt.newValue;
            SkillEditorWindows.Instance.SaveConfig();
            currentTrack.ResetView();
        });
        root.Add(jump);
    }
}