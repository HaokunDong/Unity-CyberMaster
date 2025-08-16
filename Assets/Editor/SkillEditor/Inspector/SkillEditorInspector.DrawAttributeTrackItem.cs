using UnityEngine;
using UnityEngine.UIElements;

partial class SkillEditorInspector
{
    private AttributeTrackItem currentAttributeTrackItem;

    private void DrawTrackItem(AttributeTrackItem item)
    {
        currentAttributeTrackItem = item;
        //轨道长度
        durationField = new IntegerField("片段帧数");
        durationField.value = item.Clip.DurationFrame;
        durationField.RegisterValueChangedCallback(TrackDurationFieldValueChangedCallback<AttributeTrack, AttributeTrackItem, SkillAttributeClip>);
        root.Add(durationField);

        Button addButton = new Button(AddASkillAttribute);
        addButton.text = "新增一条属性";
        addButton.style.backgroundColor = new Color(0, 1, 0, 0.5f);
        root.Add(addButton);

        if(item.Clip.AddSkillAttributes != null)
        {
            var len = item.Clip.AddSkillAttributes.Count;
            for(int i = 0; i < len; i++)
            {
                var data = item.Clip.AddSkillAttributes[i];
                var index = i;
                var delButton = new Button(() =>
                {
                    item.Clip.AddSkillAttributes.RemoveAt(index);
                    currentTrack.ResetView();
                    Show();
                });
                delButton.text = "删除此条属性";
                delButton.style.backgroundColor = new Color(1, 0, 1, 0.5f);
                root.Add(delButton);
                var space = new Label();

                var gpaField = new EnumField("属性类型", data.GPAType);
                gpaField.value = data.GPAType;
                gpaField.RegisterValueChangedCallback(evt =>
                {
                    data.GPAType = (GamePlayAttributeType)evt.newValue;
                    SkillEditorWindows.Instance.SaveConfig();
                    currentTrack.ResetView();
                    Show();
                });
                root.Add(gpaField);

                var gpaaField = new EnumField("添加方式", data.GPAAType);
                gpaaField.value = data.GPAAType;
                gpaaField.RegisterValueChangedCallback(evt =>
                {
                    data.GPAAType = (GamePlayAttributeAddType)evt.newValue;
                    SkillEditorWindows.Instance.SaveConfig();
                    currentTrack.ResetView();
                    Show();
                });
                root.Add(gpaaField);

                if(data.GPAType != GamePlayAttributeType.Invincible)
                {
                    var valueField = new FloatField("值");
                    valueField.value = data.value;
                    valueField.RegisterValueChangedCallback((ChangeEvent<float> evt) =>
                    {
                        data.value = evt.newValue;
                        SkillEditorWindows.Instance.SaveConfig();
                        currentTrack.ResetView();
                    });
                    root.Add(valueField);
                }
                
                if(data.GPAAType != GamePlayAttributeAddType.SkillFrame)
                {
                    var durationField = new FloatField("时长");
                    durationField.value = data.duration;
                    durationField.RegisterValueChangedCallback((ChangeEvent<float> evt) =>
                    {
                        data.duration = evt.newValue;
                        SkillEditorWindows.Instance.SaveConfig();
                        currentTrack.ResetView();
                    });
                    root.Add(durationField);
                }

                root.Add(space);
            }
        }
    }

    private void AddASkillAttribute()
    {
        currentAttributeTrackItem.Clip.AddSkillAttributes ??= new();
        currentAttributeTrackItem.Clip.AddSkillAttributes.Add(new());

        SkillEditorWindows.Instance.SaveConfig();
        currentTrack.ResetView();
        Show();
    }
}
