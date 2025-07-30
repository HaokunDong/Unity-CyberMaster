using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillEditorWindows))]
public partial class SkillEditorInspector : Editor
{
    public static SkillEditorInspector Instance;
    public static EditorSkillTrackBase currentTrack;
    private static TrackItemBase currentTrackItem;
    private VisualElement root;

    public static void SetTrackItem(TrackItemBase trackItem, EditorSkillTrackBase track)
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
        trackItemFrameIndex = currentTrackItem.FrameIndex;
        root.Clear();

        if (currentTrackItem is AnimationTrackItem at)
        {
            DrawTrackItem(at);
        }
        else if(currentTrackItem is HitBoxTrackItem ht)
        {
            DrawTrackItem(ht);
        }
        else if(currentTrackItem is VelocityTrackItem vt)
        {
            DrawTrackItem(vt);
        }
        else if (currentTrackItem is AttackTimeWindowTrackItem atwt)
        {
            DrawTrackItem(atwt);
        }
        else if( currentTrackItem is BlockBoxTrackItem blockItem)
        {
            DrawTrackItem(blockItem);
        }
        else if (currentTrackItem is JumpFrameTrackItem jumpFrameTrackItem)
        {
            DrawTrackItem(jumpFrameTrackItem);
        }
        else if (currentTrackItem is PlayerInputTrackItem playerInputTrackItem)
        {
            DrawTrackItem(playerInputTrackItem);
        }

        //删除
        Button deleteButton = new Button(DeleteButtonClick);
        deleteButton.text = "删除";
        deleteButton.style.backgroundColor = new Color(1, 0, 0, 0.5f);
        root.Add(deleteButton);
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

    private void TrackDurationFieldValueChangedCallback<T, I, S>(ChangeEvent<int> evt) where T : EditorSkillTrackBase where I : TrackItemBase<T, S> where S : SkillClipBase
    {
        int value = evt.newValue;

        //安全校验
        var track = currentTrack as T;
        if (track.CheckFrameIndexOnDrag(trackItemFrameIndex + value, trackItemFrameIndex, false))
        {
            var item = currentTrackItem as I;
            item.Clip.DurationFrame = value;
            item.CheckFrameCount();
            SkillEditorWindows.Instance.SaveConfig();//注意要最后保存，不然新旧数据会对不上
            currentTrackItem.ResetView();
        }
        else
        {
            durationField.value = evt.previousValue;
        }
    }

    private void DeleteButtonClick()
    {
        if (currentTrack != null)
        {
            SkillEditorWindows.Instance.OnDelItem();
            currentTrack.DeleteTrackItem(trackItemFrameIndex); //此函数提供数据保存和刷新视图的逻辑
            Selection.activeObject = null;
            currentTrack.ResetView();
        }
    }
}
