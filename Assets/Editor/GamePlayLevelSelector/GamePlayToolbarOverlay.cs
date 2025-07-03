#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;

[Overlay(typeof(SceneView), "GamePlay Toolbar", defaultDisplay = true)]
public class GamePlayToolbarOverlay : ToolbarOverlay
{
    public GamePlayToolbarOverlay() : base(GamePlayLevelIdDropdown.id) { }
}
#endif
