using Cysharp.Text;
using UnityEngine;
using UnityEngine.UIElements;

partial class SkillEditorInspector
{
    private BlockBoxTrackItem currentBlockBoxTrackItem;

    private void DrawTrackItem(BlockBoxTrackItem item)
    {
        currentBlockBoxTrackItem = item;
        var frameCountField = new IntegerField("片段帧数");
        frameCountField.value = item.Clip.DurationFrame;
        frameCountField.RegisterValueChangedCallback(TrackDurationFieldValueChangedCallback<BlockBoxTrack, BlockBoxTrackItem, SkillBlockBoxClip>);
        root.Add(frameCountField);

        Button addButton = new Button(AddASkillBlockBox);
        addButton.text = "新增一个Box";
        addButton.style.backgroundColor = new Color(0, 1, 0, 0.5f);
        root.Add(addButton);

        boxIndex = 0;
        foreach (var box in item.Clip.BlockBoxs)
        {
            box.boxIndex = boxIndex;
            root.Add(new Label(ZString.Concat("Box ", boxIndex + 1)));
            var centerField = new Vector2Field("中心");
            centerField.value = box.center;
            centerField.RegisterValueChangedCallback((ChangeEvent<Vector2> evt) =>
            {
                var changeIndex = box.boxIndex;
                currentBlockBoxTrackItem.Clip.BlockBoxs[changeIndex].center = evt.newValue;
                SkillEditorWindows.Instance.SaveConfig();
                currentTrack.ResetView();
            });
            root.Add(centerField);

            var sizeField = new Vector2Field("大小");
            sizeField.value = box.size;
            sizeField.RegisterValueChangedCallback((ChangeEvent<Vector2> evt) =>
            {
                var changeIndex = box.boxIndex;
                currentBlockBoxTrackItem.Clip.BlockBoxs[changeIndex].size = evt.newValue;
                SkillEditorWindows.Instance.SaveConfig();
                currentTrack.ResetView();
            });
            root.Add(sizeField);

            var rotationField = new FloatField("旋转");
            rotationField.value = box.rotation;
            rotationField.RegisterValueChangedCallback((ChangeEvent<float> evt) =>
            {
                var changeIndex = box.boxIndex;
                currentBlockBoxTrackItem.Clip.BlockBoxs[changeIndex].rotation = evt.newValue;
                SkillEditorWindows.Instance.SaveConfig();
                currentTrack.ResetView();
            });
            root.Add(rotationField);

            //var selectBtn = new Button(() =>
            //{
            //    SkillEditorWindows.Instance.SelectAHitBox(box.boxIndex);
            //});
            //selectBtn.text = "选中此Box";
            //selectBtn.style.backgroundColor = new Color(1, 1, 0, 0.5f);
            //root.Add(selectBtn);

            var copyBtn = new Button(() =>
            {
                CopyBox.center = box.center;
                CopyBox.size = box.size;
                CopyBox.rotation = box.rotation;
            });
            copyBtn.text = "复制此Box";
            copyBtn.style.backgroundColor = new Color(0, 1, 1, 0.5f);
            root.Add(copyBtn);

            var delButton = new Button(() =>
            {
                var delIndex = box.boxIndex;
                currentBlockBoxTrackItem.Clip.BlockBoxs.RemoveAt(delIndex);
                currentTrack.ResetView();
                Show();
            });
            delButton.text = "删除此Box";
            delButton.style.backgroundColor = new Color(1, 0, 1, 0.5f);
            root.Add(delButton);
            var space = new Label();
            root.Add(space);
            boxIndex++;
        }

        var pasteBtn = new Button(() =>
        {
            AddFromCopy = true;
            AddASkillBlockBox();
        });
        pasteBtn.text = "黏贴之前复制的Box";
        pasteBtn.style.backgroundColor = new Color(0, 1, 1, 0.5f);
        root.Add(pasteBtn);
    }

    private void AddASkillBlockBox()
    {
        var boxes = currentBlockBoxTrackItem.Clip.BlockBoxs;
        var box = new Box();
        if (AddFromCopy)
        {
            box.center = CopyBox.center;
            box.size = CopyBox.size;
            box.rotation = CopyBox.rotation;
        }
        boxes.Add(box);
        AddFromCopy = false;

        SkillEditorWindows.Instance.SaveConfig();
        currentTrack.ResetView();
        Show();
    }
}