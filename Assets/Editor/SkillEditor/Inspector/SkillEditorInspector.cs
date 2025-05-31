using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

[CustomEditor(typeof(SkillEditorWindows))]
public partial class SkillEditorInspector : Editor
{
    public static SkillEditorInspector Instance;
    private static TrackItemBase currentTrackItem;
    private static SkillTrackBase currentTrack;

    private VisualElement root;


    public static void SetTrackItem(TrackItemBase trackItem, SkillTrackBase track)
    {
        if (currentTrackItem != null)
        {
            currentTrackItem.OnUnSelect();
        }

        currentTrackItem = trackItem;
        currentTrackItem.OnSelect();
        currentTrack = track;

        //避免已经打开Inspector，导致的面板刷新不及时
        if (Instance != null) Instance.Show();
    }

    private void OnDestroy()
    {
        //说明窗口卸载
        if (currentTrackItem != null)
        {
            currentTrackItem.OnUnSelect();
            currentTrackItem = null;
            currentTrack = null;
        }
    }

    public override VisualElement CreateInspectorGUI()
    {
        Instance = this;
        root = new VisualElement();
        //root.Add(new Label("AAAA"));

        Show();
        return root;
    }

    private void Show()
    {
        Clean();
        if (currentTrackItem == null) return;
        SkillEditorWindows.Instance.CurrentSelectFrameIndex = currentTrackItem.FrameIndex;
        if (currentTrackItem is AnimationTrackItem at)
        {
            DrawAnimationTrackItem(at);
        }
        else if(currentTrackItem is HitBoxTrackItem ht)
        {
            DrawHitBoxTrackItem(ht);
        }
    }

    private void Clean()
    {
        if (root != null)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                root.RemoveAt(i);
            }
        }
    }

    private int trackItemFrameIndex; //轨道对应的帧索引
    public void SetTrackItemFrameIndex(int trackItemFrameIndex)
    {
        this.trackItemFrameIndex = trackItemFrameIndex;
    }
}
