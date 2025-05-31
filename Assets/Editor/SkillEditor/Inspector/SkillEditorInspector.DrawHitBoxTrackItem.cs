using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        SkillEditorWindows.Instance.SetDrawHitBoxes(item.SkillHitBoxClip.HitBoxs);
        root.Clear();
        currentItem = item;
        trackItemFrameIndex = item.FrameIndex;

        var frameCountField = new IntegerField("片段帧数");
        frameCountField.value = item.SkillHitBoxClip.DurationFrame;
        frameCountField.RegisterValueChangedCallback(HitBoxTrackDurationFieldValueChangedCallback);
        root.Add(frameCountField);

        Button addButton = new Button(AddABox);
        addButton.text = "新增一个Box";
        addButton.style.backgroundColor = new Color(0, 1, 0, 0.5f);
        root.Add(addButton);

        boxIndex = 0;
        foreach (var box in item.SkillHitBoxClip.HitBoxs)
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
                currentItem.SkillHitBoxClip.HitBoxs[changeIndex] = b;
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
                currentItem.SkillHitBoxClip.HitBoxs[changeIndex] = b;
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
                currentItem.SkillHitBoxClip.HitBoxs[changeIndex] = b;
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
                currentItem.SkillHitBoxClip.HitBoxs.RemoveAt(delIndex);
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
        var boxes = currentItem.SkillHitBoxClip.HitBoxs;
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

    private void HitBoxTrackDurationFieldValueChangedCallback(ChangeEvent<int> evt)
    {
        int value = evt.newValue;

        //安全校验
        var ht = currentTrack as HitBoxTrack;
        if (ht.CheckFrameIndexOnDrag(ht.HitBoxData.skillHitBoxClipDict, trackItemFrameIndex + value, trackItemFrameIndex, false))
        {
            var hi = currentTrackItem as HitBoxTrackItem;
            hi.SkillHitBoxClip.DurationFrame = value;
            hi.CheckFrameCount();
            SkillEditorWindows.Instance.SaveConfig();//注意要最后保存，不然新旧数据会对不上
            currentTrackItem.ResetView();
        }
        else
        {
            durationField.value = evt.previousValue;
        }
    }
}
