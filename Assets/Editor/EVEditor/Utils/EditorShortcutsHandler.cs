using Tools;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace EverlastingEditor.Utils
{
    public class EditorShortcutsHandler
    {
        [InitializeOnLoadMethod]
        private static void InitShortcuts()
        {
            EditorShortcutsManager.Register(KeyCode.PageUp, ShortcutModifiers.Action, RunGmTest);
        }

        private static void RunGmTest()
        {
#if DEBUG_ASSIST_ENABLE
            GmUtils.TestDebug();
#endif
        }
    }
}