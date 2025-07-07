using Cysharp.Text;
using GameBase.Log;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockBoxTrack : EditorSkillTrackBase<SkillBlockBoxClip>
{
    protected override void CreateItem(int frameIndex, SkillBlockBoxClip clip)
    {
        BlockBoxTrackItem trackItem = new BlockBoxTrackItem();
        trackItem.Init(this, trackStyle, frameIndex, frameWidth, clip, new Color(0.850f, 0.0f, 0.388f, 0.5f), new Color(0.850f, 0.0f, 0.388f, 1f));
        trackItemDic.Add(frameIndex, trackItem.itemStyle);
    }

    private Vector2 mouseLocalPos;

    protected override void OnPointerDownEvent(MouseDownEvent evt)
    {
        if (evt.button == 1)//右键
        {
            if (!IsClipEmptyAndNotOverLength(evt))
            {
                return;
            }
            mouseLocalPos = evt.localMousePosition;
            ShowContextMenu(evt);
        }
    }

    private void ShowContextMenu(MouseDownEvent evt)
    {
        GenericMenu menu = new GenericMenu();

        int selectFrameIndex = SkillEditorWindows.Instance.GetFrameIndexByPos(mouseLocalPos.x);
        // 添加菜单项
        menu.AddItem(new GUIContent("新增"), false, () =>
        {
            SkillBlockBoxClip clip = new SkillBlockBoxClip();
            clip.DurationFrame = 1;
            skillClipDict.Add(selectFrameIndex, clip);
            SkillEditorWindows.Instance.SaveConfig();
            CreateItem(selectFrameIndex, clip);
        });
        menu.AddItem(new GUIContent("补间"), false, () =>
        {
            TryLerp(selectFrameIndex);
        });
        // 在鼠标位置显示菜单
        menu.DropDown(new Rect(evt.mousePosition, Vector2.zero));
    }

    protected override void Lerp(SkillBlockBoxClip leftC, SkillBlockBoxClip rightC, int leftStartFrame, int rightEndFrame)
    {
        if (leftC.BlockBoxs == null || leftC.BlockBoxs.Count <= 0 || rightC.BlockBoxs == null || rightC.BlockBoxs.Count <= 0 || leftC.BlockBoxs.Count != rightC.BlockBoxs.Count)
        {
            LogUtils.Error(ZString.Concat("第", leftStartFrame - 1, "帧和第", rightEndFrame + 1, "帧的BlockBoxs数量不一致或者至少一方为0个，无法自动补间"));
            return;
        }

        for (int i = leftStartFrame; i <= rightEndFrame; i++)
        {
            SkillBlockBoxClip clip = new SkillBlockBoxClip();
            clip.DurationFrame = 1;
            for (int j = 0; j < leftC.BlockBoxs.Count; j++)
            {
                Box b = new Box();
                b.size = Vector2.Lerp(leftC.BlockBoxs[j].size, rightC.BlockBoxs[j].size, (i - leftStartFrame + 1) / (float)(rightEndFrame - leftStartFrame + 2));
                b.center = Vector2.Lerp(leftC.BlockBoxs[j].center, rightC.BlockBoxs[j].center, (i - leftStartFrame + 1) / (float)(rightEndFrame - leftStartFrame + 2));
                b.rotation = Mathf.LerpAngle(leftC.BlockBoxs[j].rotation, rightC.BlockBoxs[j].rotation, (i - leftStartFrame + 1) / (float)(rightEndFrame - leftStartFrame + 2));
                clip.BlockBoxs.Add(b);
            }
            skillClipDict.Add(i, clip);
            CreateItem(i, clip);
        }
        SkillEditorWindows.Instance.SaveConfig();
    }
}
