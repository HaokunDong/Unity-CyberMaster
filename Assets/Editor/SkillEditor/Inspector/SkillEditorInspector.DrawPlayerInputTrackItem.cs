using UnityEditor.UIElements;
using UnityEngine.UIElements;

partial class SkillEditorInspector
{
    private PlayerInputTrackItem playerInputTrackItem;

    private void DrawTrackItem(PlayerInputTrackItem item)
    {
        playerInputTrackItem = item;
        var frameCountField = new IntegerField("片段帧数");
        frameCountField.value = item.Clip.DurationFrame;
        frameCountField.RegisterValueChangedCallback(TrackDurationFieldValueChangedCallback<PlayerInputTrack, PlayerInputTrackItem, SkillPlayerInputClip>);
        root.Add(frameCountField);

        var flagField = new EnumFlagsField("输入能执行哪些操作", item.Clip.inputToDoFlags);
        flagField.value = item.Clip.inputToDoFlags;
        flagField.RegisterValueChangedCallback(evt =>
        {
            item.Clip.inputToDoFlags = (InputToDoFlags)evt.newValue;
            SkillEditorWindows.Instance.SaveConfig();
            currentTrack.ResetView();
        });
        root.Add(flagField);

        if (item.Clip.inputToDoFlags.Has(InputToDoFlags.NextSkillWithTailCut))
        {
            var nextSkillTailCutFrameField = new IntegerField("当前技能最多播放到第几帧");
            nextSkillTailCutFrameField.value = item.Clip.NextSkillTailCutFrame;
            nextSkillTailCutFrameField.RegisterValueChangedCallback(evt =>
            {
                item.Clip.NextSkillTailCutFrame = evt.newValue;
                SkillEditorWindows.Instance.SaveConfig();
                currentTrack.ResetView();
            });
            root.Add(nextSkillTailCutFrameField);
        }
    }
}
