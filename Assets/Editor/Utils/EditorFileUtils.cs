using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    public static class EditorFileUtils
    {
        public static string SubStringToAssets(string path)
        {
            return path.Substring(path.IndexOf("Assets", StringComparison.Ordinal));
        }
        
        //返回true表示需要保存prefab
        public static void ForeachPrefab(string dirPath, Func<string, GameObject, bool> callback)
        {
            if (!Directory.Exists(dirPath))
            {
                Debug.LogError($"找不到对应文件夹:{dirPath}");
                return;
            }

            bool needRefresh = false;
            foreach (var filePath in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
            {
                if (filePath.EndsWith(".prefab"))
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(SubStringToAssets(filePath));
                    if (callback(filePath, go))
                    {
                        needRefresh = true;
                        PrefabUtility.SavePrefabAsset(go);
                    }
                }
            }

            if (needRefresh)
            {
                AssetDatabase.Refresh();
            }
        }
        
        public static void ForeachSprite(string dirPath, Action<string, Sprite> callback)
        {
            if (!Directory.Exists(dirPath))
            {
                Debug.LogError($"找不到对应文件夹:{dirPath}");
                return;
            }

            foreach (var filePath in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
            {
                if (filePath.EndsWith(".jpg") || filePath.EndsWith(".png"))
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SubStringToAssets(filePath));
                    callback(filePath, sprite);
                }
            }
        }
        
        public static void ForeachFile(string dirPath, Action<string> callback, string pattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (!Directory.Exists(dirPath))
            {
                Debug.LogError($"找不到对应文件夹:{dirPath}");
                return;
            }

            foreach (var filePath in Directory.GetFiles(dirPath, pattern, searchOption))
            {
                callback(filePath);
            }
        }

        public static void ForeachScriptableObjects<T>(string dirPath, Action<T> callback) where T : ScriptableObject
        {
            if (!Directory.Exists(dirPath))
            {
                Debug.LogError($"找不到对应文件夹:{dirPath}");
                return;
            }

            foreach (var filePath in Directory.GetFiles(dirPath, "*.asset", SearchOption.AllDirectories))
            {
                var scriptableObject = AssetDatabase.LoadAssetAtPath<T>(SubStringToAssets(filePath));
                callback(scriptableObject);
            }
        }

        public static string FindFileInDirByName(string dirPath, string fileName)
        {
            DirectoryInfo di = new DirectoryInfo(dirPath);
            if (di.Exists)
            {
                var files = di.GetFiles(fileName, SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    return files[0].FullName;
                }
            }
            return null;
        }
        
        public static string GetRelativePath(string fullPath, string basePath)
        {
            if (!basePath.EndsWith("\\") || !basePath.EndsWith("/"))
            {
                basePath += "/";
            }

            var baseUri = new Uri(basePath);
            var fullUri = new Uri(fullPath);
            var relativeUri = baseUri.MakeRelativeUri(fullUri);
            return relativeUri.ToString();
        }
        
    }
}