using Cysharp.Text;
using UnityEngine.UIElements;

partial class SkillEditorInspector
{
    private void DrawTrackItem(AttackTimeWindowTrackItem item)
    {
        trackItemFrameIndex = item.FrameIndex;
        root.Clear();

        durationField = new IntegerField("片段帧数");
        durationField.value = item.Clip.DurationFrame;
        durationField.RegisterValueChangedCallback(TrackDurationFieldValueChangedCallback<AttackTimeWindowTrack, AttackTimeWindowTrackItem, SkillAttackTimeWindowClip>);
        root.Add(durationField);

        var label = new Label(ZString.Concat("从第", trackItemFrameIndex, "帧到第", trackItemFrameIndex + item.Clip.DurationFrame, "帧之内的所有打击算为同一刀，最多造成一次命中判定。"));
        root.Add(label);
    }
}
