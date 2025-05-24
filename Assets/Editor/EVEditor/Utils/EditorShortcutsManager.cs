using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace EverlastingEditor.Utils
{
    public static class EditorShortcutsManager
    {
        private static List<(KeyCode, ShortcutModifiers, Action)> shortcuts = new
            List<(KeyCode, ShortcutModifiers, Action)>();


        [InitializeOnLoadMethod]
        private static void RegisterDelegate()
        {
            FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler",
                BindingFlags.Static | BindingFlags.NonPublic);

            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction) info.GetValue(null);

            value += EditorGlobalKeyPress; 

            info.SetValue(null, value);
        }

        private static void EditorGlobalKeyPress()
        {
            var @event = Event.current;

            if (@event.type == EventType.KeyDown)
            {
                ShortcutModifiers modifiers = 0;
                if (@event.alt)
                    modifiers |= ShortcutModifiers.Alt;
                if (@event.control)
                    modifiers |= ShortcutModifiers.Action;
                if (@event.shift)
                    modifiers |= ShortcutModifiers.Shift;
                bool used = false;
                foreach (var shortcut in shortcuts)
                {
                    if (@event.keyCode == shortcut.Item1 && modifiers == shortcut.Item2)
                    {
                        shortcut.Item3();
                        used = true;
                    }
                }

                if (used)
                    @event.Use();
            }
        }

        public static void Register(KeyCode keyCode, ShortcutModifiers shortcutModifiers, Action action)
        {
            shortcuts.Add((keyCode, shortcutModifiers, action));
        }
    }
}