using UnityEngine;
using UnityEngine.UIElements;

partial class SkillEditorInspector
{
    private void DrawTrackItem(AttributeTrackItem item)
    {
        //轨道长度
        durationField = new IntegerField("片段帧数");
        durationField.value = item.Clip.DurationFrame;
        durationField.RegisterValueChangedCallback(TrackDurationFieldValueChangedCallback<AttributeTrack, AttributeTrackItem, SkillAttributeClip>);
        root.Add(durationField);

        var isInvincible = new Toggle("是否无敌");
        isInvincible.value = item.Clip.IsInvincible;
        isInvincible.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
        {
            item.Clip.IsInvincible = evt.newValue;
            SkillEditorWindows.Instance.SaveConfig();
        });
        root.Add(isInvincible);

    }
}
