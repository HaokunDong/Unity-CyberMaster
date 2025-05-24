using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Everlasting.Extend;
using Everlasting.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = UnityEditor.Compilation.Assembly;

namespace EverlastingEditor.Utils
{
    public class EditorUtils
    {

        public static string[] GetFiles(string subPath, string searchPattern)
        {
            return Directory.GetFiles(subPath, searchPattern,
                    SearchOption.AllDirectories)
                .Select(s => s.Replace("\\", "/")).ToArray();
        }

        public static string[] GetFilesRelative(string subPath, string searchPattern)
        {
            var relativeLength = subPath.Length + (subPath.EndsWith("/") || subPath.EndsWith("\\") ? 0 : 1);
            return Directory.GetFiles(subPath, searchPattern,
                    SearchOption.AllDirectories)
                .Select(s => s.Remove(0, relativeLength)).Select(s => s.Replace("\\", "/"))
                .ToArray();
        }

        public static string[] GetPrefabs(string subPath)
        {
            return GetFiles(subPath, "*.prefab");
        }

        public static string[] GetUIPrefabs()
        {
            return GetPrefabs("Assets/Res/Prefabs/UI");
        }

        /// <summary>
        /// 遍历路径，加载prefab
        /// </summary>
        /// <param name="paths">prefab路径</param>
        /// <param name="func">返回true，则表示prefab被修改，需要保存</param>
        public static void ForeachPrefabs(IEnumerable<string> paths, Func<GameObject, bool> func)
        {
            foreach (var path in paths)
            {
                GameObject go = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (func(go))
                    {
                        go = PrefabUtility.SavePrefabAsset(go);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(go);
                }
            }
        }

        public static void ForeachChildrenDo(GameObject go, Action<GameObject> action)
        {
            action(go);
            foreach (Transform child in go.transform)
            {
                ForeachChildrenDo(child.gameObject, action);
            }
        }

        public static void DisplayCancelableProgressBar(string title, string info, float progress)
        {
            if (Application.isBatchMode)
                return;
            if (EditorUtility.DisplayCancelableProgressBar(title, info, progress))
            {
                throw new OperationCanceledException();
            }
        }

        public static void ClearProgressBar()
        {
            if (Application.isBatchMode)
                return;
            EditorUtility.ClearProgressBar();
        }

        public static List<Type> GetAllTypes(AssembliesType assembliesType)
        {
            Assembly[] assemblies = CompilationPipeline.GetAssemblies(assembliesType);
            HashSet<string> assemblyNames = assemblies.Select(a => a.name).ToHashSet();
            List<Type> types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assemblyNames.Contains(assembly.GetName().Name))
                {
                    types.AddRange(assembly.GetTypes());
                }
            }

            return types;
        }

        public static List<Type> GetAllRuntimeTypes()
        {
            return GetAllTypes(AssembliesType.Player);
        }

        public static List<Type> GetAllRuntimeTypesWithAttribute<T>() where T : Attribute
        {
            List<Type> allTypes = GetAllRuntimeTypes();
            return allTypes.Where(t => t.GetCustomAttribute<T>() != null).ToList();
        }
        
        public static void TriggerOnValidate(GameObject go)
        {
            ForeachChildrenDo(go, o =>
            {
                using (var components = o.GetComponentsNonAlloc<MonoBehaviour>())
                {
                    foreach (var component in components)
                    {
                        ReflectionCache.GetPrivateMethodInfos(component.GetType()).GetValueOrDefaultEx("OnValidate")
                            ?.Invoke(component, null);
                        ReflectionCache.GetPublicMethodInfos(component.GetType()).GetValueOrDefaultEx("OnValidate")
                            ?.Invoke(component, null);
                    }
                }
            });
        }
    }
}