using Cysharp.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

partial class SkillEditorInspector
{
    private static Box CopyBox = new Box();
    private static bool AddFromCopy = false;

    private HitBoxTrackItem currentItem;
    private int boxIndex;

    private void DrawHitBoxTrackItem(HitBoxTrackItem item)
    {
        root.Clear();
        currentItem = item;
        trackItemFrameIndex = item.FrameIndex;

        var frameCountField = new IntegerField("片段帧数");
        frameCountField.value = item.Clip.DurationFrame;
        frameCountField.RegisterValueChangedCallback(TrackDurationFieldValueChangedCallback<HitBoxTrack, HitBoxTrackItem, SkillHitBoxClip>);
        root.Add(frameCountField);

        var layer = new LayerMaskField("碰撞层");
        layer.value = item.Clip.layer;
        layer.RegisterValueChangedCallback(evt =>
        {
            item.Clip.layer = evt.newValue;
            SkillEditorWindows.Instance.SaveConfig();
        });
        root.Add(layer);

        Button addButton = new Button(AddABox);
        addButton.text = "新增一个Box";
        addButton.style.backgroundColor = new Color(0, 1, 0, 0.5f);
        root.Add(addButton);

        boxIndex = 0;
        foreach (var box in item.Clip.HitBoxs)
        {
            box.boxIndex = boxIndex;
            root.Add(new Label(ZString.Concat("Box ", boxIndex + 1)));
            var centerField = new Vector2Field("中心");
            centerField.value = box.center;
            centerField.RegisterValueChangedCallback((ChangeEvent<Vector2> evt) =>
            {
                var changeIndex = box.boxIndex;
                Box b = new Box
                {
                    center = evt.newValue,
                    size = box.size,
                    rotation = box.rotation,
                    boxIndex = changeIndex
                };
                currentItem.Clip.HitBoxs[changeIndex] = b;
                SkillEditorWindows.Instance.SaveConfig();
                currentTrack.ResetView();
                SceneReDrawHitBoxes();
            });
            root.Add(centerField);

            var sizeField = new Vector2Field("大小");
            sizeField.value = box.size;
            sizeField.RegisterValueChangedCallback((ChangeEvent<Vector2> evt) => 
            {
                var changeIndex = box.boxIndex;
                Box b = new Box
                {
                    center = box.center,
                    size = evt.newValue,
                    rotation = box.rotation,
                    boxIndex = changeIndex
                };
                currentItem.Clip.HitBoxs[changeIndex] = b;
                SkillEditorWindows.Instance.SaveConfig();
                currentTrack.ResetView();
                SceneReDrawHitBoxes();
            });
            root.Add(sizeField);

            var rotationField = new FloatField("旋转");
            rotationField.value = box.rotation;
            rotationField.RegisterValueChangedCallback((ChangeEvent<float> evt) =>
            {
                var changeIndex = box.boxIndex;
                Box b = new Box
                {
                    center = box.center,
                    size = box.size,
                    rotation = evt.newValue,
                    boxIndex = changeIndex
                };
                currentItem.Clip.HitBoxs[changeIndex] = b;
                SkillEditorWindows.Instance.SaveConfig();
                currentTrack.ResetView();
                SceneReDrawHitBoxes();
            });
            root.Add(rotationField);

            var selectBtn = new Button(() => 
            {
                SkillEditorWindows.Instance.SelectAHitBox(box.boxIndex);
            });
            selectBtn.text = "选中此Box";
            selectBtn.style.backgroundColor = new Color(1, 1, 0, 0.5f);
            root.Add(selectBtn);

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
                currentItem.Clip.HitBoxs.RemoveAt(delIndex);
                currentTrack.ResetView();
                DrawHitBoxTrackItem(currentItem);
                SceneReDrawHitBoxes();
            });
            delButton.text = "删除此Box";
            delButton.style.backgroundColor = new Color(1, 0, 1, 0.5f);
            root.Add(delButton);
            root.Add(new Label());
            boxIndex++;
        }

        var pasteBtn = new Button(() =>
        {
            AddFromCopy = true;
            AddABox();
        });
        pasteBtn.text = "黏贴之前复制的Box";
        pasteBtn.style.backgroundColor = new Color(0, 1, 1, 0.5f);
        root.Add(pasteBtn);

        Button deleteButton = new Button(DeleteButtonClick);
        deleteButton.text = "删除整个片段";
        deleteButton.style.backgroundColor = new Color(1, 0, 0, 0.5f);
        root.Add(deleteButton);
    }

    private void AddABox()
    {
        var boxes = currentItem.Clip.HitBoxs;
        var box = new Box();
        if(AddFromCopy)
        {
            box.center = CopyBox.center;
            box.size = CopyBox.size;
            box.rotation = CopyBox.rotation;
        }
        boxes.Add(box);
        AddFromCopy = false;

        SkillEditorWindows.Instance.SaveConfig();
        currentTrack.ResetView();
        DrawHitBoxTrackItem(currentItem);
        SceneReDrawHitBoxes();
    }

    private void SceneReDrawHitBoxes()
    {
        EditorApplication.delayCall += () =>
        {
            DrawHitBoxTrackItem(currentItem);
        };
    }
}
