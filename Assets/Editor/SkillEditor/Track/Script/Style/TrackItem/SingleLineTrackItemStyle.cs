using UnityEditor;
using UnityEngine.UIElements;

public class SingleLineTrackItemStyle : SkillTrackItemStyleBase
{
    private const string trackItemAssetPath = "Assets/Editor/SkillEditor/Track/UXMLs/SingleLineTrackStyle/SingleLineTrackStyle.uxml";
    
    public override void Init(SkillTrackStyleBase TrackStyle, int startFrameIndex, float frameUnitWidth)
    {
        root = titleLabel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(trackItemAssetPath).Instantiate().Query<Label>();//不要容器，直接持有目标物体
        mainDragArea = root.Q<VisualElement>("Main");
        overLine = root.Q<VisualElement>("OverLine");
        TrackStyle.AddItem(root);
    }

    public void SetTitle(string title)
    {
        titleLabel.text = title;
    }
}
