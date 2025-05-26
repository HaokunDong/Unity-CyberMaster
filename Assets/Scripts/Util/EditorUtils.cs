#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tools
{
    public static class EditorUtils
    {
        public static T[] FindComponentsInSceneOrPrefabStage<T>() where T : Component
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var objs = prefabStage != null ? prefabStage.FindComponentsOfType<T>() :
                GameObject.FindObjectsOfType<T>();
            return objs;
        }
        
        //遍历执行子节点action，直到返回true
        public static void ForeachChildrenDoUntil(GameObject go, Func<GameObject, bool> action)
        {
            if (action(go)) return;
            //有GC，引以为戒
            // foreach (Transform child in go.transform)
            // {
            //     ForeachChildrenDoUntil(child.gameObject, action);
            // }

            for (int i = 0, max = go.transform.childCount; i < max; i++)
            {
                var child = go.transform.GetChild(i);
                ForeachChildrenDoUntil(child.gameObject, action);
            }
        }

        //深度遍历子节点，但如果遇到prefab则不往下遍历
        public static void ForeachChildNotPrefabInstance(GameObject go, Action<GameObject> action, bool includeInActive = false)
        {
            EditorUtils.ForeachChildrenDoUntil(go, child =>
            {
                //遇到递归prefab直接返回
                if(PrefabUtility.IsAnyPrefabInstanceRoot(child) && child != go) return true;
                if(!includeInActive && !child.activeSelf) return true;
                action(child);
                return false;
            });
        }

        public static void ShowAddComponentSelectMenu(GameObject go, IEnumerable<GenericSelectorItem<Type>> selectorItems)
        {
            var selector = new GenericSelector<Type>(null, false, selectorItems);
            selector.SelectionTree.EnumerateTree().AddThumbnailIcons();
            selector.SelectionConfirmed += types =>
            {
                var type = types.FirstOrDefault();
                if (type != null)
                {
                    go.AddComponent(type);
                }
            };
            selector.ShowInPopup();
        }
    }
}
#endif