#if UNITY_EDITOR
using Everlasting.Config;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

[EditorToolbarElement(id)]
public class GamePlayLevelIdDropdown : EditorToolbarDropdown
{
    public const string id = "GamePlay/LevelIdSelector";

    private uint currentLevelId;

    public GamePlayLevelIdDropdown()
    {
        currentLevelId = LoadSavedLevelId();
        text = $"¹Ø¿¨ {currentLevelId} {GamePlayTable.GetTableData(currentLevelId)?.GamePlayName}";
        clicked += ShowDropdown;
    }

    private void ShowDropdown()
    {
        var menu = new GenericMenu();
        var allGPs = GamePlayTable.All;
        foreach (var data in allGPs)
        {
            var id = data.Id;
            menu.AddItem(new GUIContent($"¹Ø¿¨ {id} {data.GamePlayName}"), id == currentLevelId, () =>
            {
                currentLevelId = id;
                text = $"¹Ø¿¨ {id} {data.GamePlayName}";
                SaveSelectedLevelId(currentLevelId);
            });
        }
        menu.DropDown(worldBound);
    }

    private void SaveSelectedLevelId(uint id)
    {
        EditorPrefs.SetInt("SelectedGamePlayLevelId", (int)id);
    }

    private uint LoadSavedLevelId()
    {
        return (uint)EditorPrefs.GetInt("SelectedGamePlayLevelId", 10001);
    }

    public static uint GetSelectedLevelId()
    {
        return (uint)EditorPrefs.GetInt("SelectedGamePlayLevelId", 10001);
    }
}
#endif
