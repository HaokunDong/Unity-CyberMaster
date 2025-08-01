using UnityEditor;
using UnityEngine;

namespace Griffin
{
    public class AssetBundleMenu
    {
        [MenuItem("Tools/打包/AssetBundle/Build Windows AssetBundles")]
        public static void BuildWindowsAssetBundle()
        {
            AssetBundlePack.BuildAllAssetBundle(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Tools/打包/AssetBundle/Build Android AssetBundles")]
        public static void BuildAndroidAssetBundle()
        {
            AssetBundlePack.BuildAllAssetBundle(BuildTarget.Android);
        }

        [MenuItem("Tools/打包/AssetBundle/Build IOS AssetBundles")]
        public static void BuildIOSAssetBundle()
        {
            AssetBundlePack.BuildAllAssetBundle(BuildTarget.iOS);
        }

        [MenuItem("Tools/打包/AssetBundle/清理AssetBundles和BuildCache")]
        public static void ClearAssetBundle()
        {
            AssetBundlePack.ClearAssetBundleAndCache(AssetBundleData.s_abFullFolderPath);
        }
        
        [InitializeOnLoadMethod]
        private static void CustomDefaultSceneWindowInit()
        {
            var assetBundleModeEditor = EditorPrefs.GetBool("AssetBundleModeEditor");
            EditorApplication.delayCall += () =>
            {
                Menu.SetChecked("Tools/打包/AssetBundle/编辑器加载启用ab模式", assetBundleModeEditor);
            };
        }

        [MenuItem("Tools/打包/AssetBundle/编辑器加载启用ab模式")]
        private static void LoadAssetBundleEditor()
        {
            var assetBundleModeEditor = !EditorPrefs.GetBool("AssetBundleModeEditor");
            EditorPrefs.SetBool("AssetBundleModeEditor", assetBundleModeEditor);
            Menu.SetChecked("Tools/打包/AssetBundle/编辑器加载启用ab模式", assetBundleModeEditor);
        }
        
        [MenuItem("Tools/打包/AssetBundle/清理unityab缓存")]
        private static void ClearCache()
        {
            if (Caching.ClearCache()) 
            {
                Debug.Log("Successfully cleaned the cache.");
            }
            else 
            {
                Debug.Log("Cache is being used.");
            }
        }
    }
}