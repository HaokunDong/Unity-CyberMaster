using Cysharp.Text;
using GameBase.Log;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HitBoxTrack : EditorSkillTrackBase<SkillHitBoxClip>
{
    protected override void CreateItem(int frameIndex, SkillHitBoxClip clip)
    {
        HitBoxTrackItem trackItem = new HitBoxTrackItem();
        trackItem.Init(this, trackStyle, frameIndex, frameWidth, clip, new Color(0.850f, 0.388f, 0.388f, 0.5f), new Color(0.850f, 0.388f, 0.388f, 1f));
        trackItemDic.Add(frameIndex, trackItem.itemStyle);
    }

    private Vector2 mouseLocalPos;

    protected override void OnPointerDownEvent(MouseDownEvent evt)
    {
        if (evt.button == 1)//右键
        {
            if (!IsClipEmpty(evt))
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
            SkillHitBoxClip clip = new SkillHitBoxClip();
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

    protected override void Lerp(SkillHitBoxClip leftC, SkillHitBoxClip rightC, int leftStartFrame, int rightEndFrame)
    {
        if (leftC.HitBoxs == null || leftC.HitBoxs.Count <= 0 || rightC.HitBoxs == null || rightC.HitBoxs.Count <= 0 || leftC.HitBoxs.Count != rightC.HitBoxs.Count)
        {
            LogUtils.Error(ZString.Concat("第", leftStartFrame - 1, "帧和第", rightEndFrame + 1, "帧的HitBox数量不一致或者至少一方为0个，无法自动补间"));
            return;
        }

        for (int i = leftStartFrame; i <= rightEndFrame; i++)
        {
            SkillHitBoxClip clip = new SkillHitBoxClip();
            clip.DurationFrame = 1;
            for (int j = 0; j < leftC.HitBoxs.Count; j++)
            {
                Box b = new Box();
                b.size = Vector2.Lerp(leftC.HitBoxs[j].size, rightC.HitBoxs[j].size, (i - leftStartFrame + 1) / (float)(rightEndFrame - leftStartFrame + 2));
                b.center = Vector2.Lerp(leftC.HitBoxs[j].center, rightC.HitBoxs[j].center, (i - leftStartFrame + 1) / (float)(rightEndFrame - leftStartFrame + 2));
                b.rotation = Mathf.LerpAngle(leftC.HitBoxs[j].rotation, rightC.HitBoxs[j].rotation, (i - leftStartFrame + 1) / (float)(rightEndFrame - leftStartFrame + 2));
                clip.HitBoxs.Add(b);
            }
            skillClipDict.Add(i, clip);
            CreateItem(i, clip);
        }
        SkillEditorWindows.Instance.SaveConfig();
    }
}