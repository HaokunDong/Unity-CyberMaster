#if UNITY_EDITOR
//Griffin 修改NodeCanvas插件和unity版本适配
using UnityEditor.SceneManagement;
//Griffin 修改NodeCanvas插件和unity版本适配
#endif
using UnityEngine;

#if UNITY_EDITOR
namespace ParadoxNotion.Design
{
    ///<summary>Have some commonly stuff used across most inspectors and helper functions. Keep outside of Editor folder since many runtime classes use this in #if UNITY_EDITOR. This is a partial class. Different implementation provide different tools, so that everything is referenced from within one class.</summary>
    public static partial class EditorUtils
    {
        //gx:判断节点是否为prefab模式下   
        internal static bool IsPartOfCurrentPrefabStage(this GameObject self)
        {
#if UNITY_EDITOR
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            return prefabStage != null && prefabStage.IsPartOfPrefabContents(self);
#else
            return false;
#endif
        }
    }
}
#endif