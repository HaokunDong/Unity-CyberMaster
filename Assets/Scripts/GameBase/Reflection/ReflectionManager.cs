using System;
using System.Collections.Generic;
using System.Reflection;
using Everlasting.Base;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GameBase.Reflection
{
    public static class ReflectionManager
    {
        static ReflectionManager()
        {
            Load("Everlasting.Scripts", "Everlasting.Panels", "Everlasting.Configs", "Assembly-CSharp");
        }

        private static readonly MultiDictionary<Type, Type> types = new MultiDictionary<Type, Type>();

        private static void Load(params string[] assemblyNames)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (Array.IndexOf(assemblyNames,assembly.GetName().Name) > -1)
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        foreach (var attribute in type.GetCustomAttributes<BaseAttribute>(false))
                        {
                            types.Add(attribute.GetType(), type);
                        }
                    }
                }
            }
        }

        public static List<Type> GetTypes<T>() where T : BaseAttribute
        {
            Type t = typeof(T);
            return types[t];
        }
        
#if UNITY_EDITOR
        public struct MethodReflectionInfo
        {
            public MethodInfo methodInfo;
            public Type classType;
            public EditorFunctionAttribute attribute;
        }
        private static readonly string[] TraverseEditorAssemblies = new[] { "Assembly-CSharp" , "Assembly-CSharp-Editor", "Unity.InternalAPIEngineBridge.001"};
        private static MultiDictionary<Type, MethodReflectionInfo> methodsTypes = null;
        
        private static void LoadFunctionAttrs()
        {
            methodsTypes = new MultiDictionary<Type, MethodReflectionInfo>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (Array.IndexOf(TraverseEditorAssemblies,assembly.GetName().Name) >= 0)
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        foreach (var function in type.GetMethods(BindingFlags.Instance | BindingFlags.Public |
                                                                  BindingFlags.NonPublic | BindingFlags.Static))
                        {
                            EditorFunctionAttribute attribute = function.GetCustomAttribute<EditorFunctionAttribute>(true);
                            if (attribute == null)
                            {
                                continue;
                            }
                            methodsTypes.Add(attribute.GetType(), new MethodReflectionInfo()
                            {
                                methodInfo = function,
                                classType = type,
                                attribute = attribute,
                            });
                        }
                    }
                }
            }
        }

        public static List<MethodReflectionInfo> GetEditorMethodAttrs<T>() where T : EditorFunctionAttribute
        {
            if (methodsTypes == null)
            {
                LoadFunctionAttrs();
            }
            
            methodsTypes.TryGetValue(typeof(T), out var result);
            return result;
        }
        
#endif
    }
}