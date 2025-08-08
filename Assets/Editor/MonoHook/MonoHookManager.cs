using System;
using System.Collections.Generic;
using System.Reflection;
using Everlasting.Base;
using MonoMod.RuntimeDetour;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace GameEditor.MonoHook
{
    public abstract class BaseMonoHookAttribute : Attribute
    {
        public string assembleName;
        public string className;
        public BindingFlags bindingFlags;
        public bool isNative;
    }
    
    public class MonoHookMethodAttribute : BaseMonoHookAttribute
    {
        public string methodName;
        public Type[] paramTypes;

        public MonoHookMethodAttribute(string assembleName, string className, string methodName, 
            BindingFlags bindingFlags = MonoHookManager.ALL_BINDING_FLAGS, bool isNative = false, Type[] paramTypes = null)
        {
            this.assembleName = assembleName;
            this.className = className;
            this.methodName = methodName;
            this.bindingFlags = bindingFlags;
            this.isNative = isNative;
            this.paramTypes = paramTypes;
        }
    }
    
    public class MonoHookPropertyAttribute : BaseMonoHookAttribute
    {
        public string propertyName;
        public HookType hookType;
    
        public enum HookType
        {
            GetMethod,
            SetMethod,
        }
    
        public MonoHookPropertyAttribute(string assembleName, string className, string propertyName, HookType type, BindingFlags bindingFlags = MonoHookManager.ALL_BINDING_FLAGS, bool isNative = false)
        {
            this.assembleName = assembleName;
            this.className = className;
            this.propertyName = propertyName;
            this.hookType = type;
            this.bindingFlags = bindingFlags;
            this.isNative = isNative;
        }
    }
    
    public static class MonoHookManager
    {
        public const BindingFlags ALL_BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Static |
                                                       BindingFlags.Public | BindingFlags.NonPublic;
        private static IDetour[] s_iDetours;

        public static Dictionary<string, IDetour> methodName2NativeDetour = new Dictionary<string, IDetour>();

        [InitializeOnLoadMethod]
        private static void RunOnLoad()
        {
            EditorApplication.delayCall += PatchAll;
            // PatchAll();
        }
        
        private static void PatchAll()
        {
            MultiDictionary<string, (MethodInfo methodType, BaseMonoHookAttribute attribute)> hookAttrs =
                new MultiDictionary<string, (MethodInfo methodType, BaseMonoHookAttribute attribute)>();

            foreach (var method in typeof(MonoHookImplement).GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                                        BindingFlags.NonPublic))
            {
                var methodAttribute = method.GetAttribute<BaseMonoHookAttribute>();
                if (methodAttribute != null)
                {
                    hookAttrs.Add(methodAttribute.assembleName, (method, methodAttribute));
                }
            }
            
            List<IDetour> iDeours = new List<IDetour>();
            Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var ass in asses)
            {
                if (hookAttrs.TryGetValue(ass.GetName().Name, out var list))
                {
                    foreach (var (methodTarget, attribute) in list)
                    {
                        var classType = ass.GetType(attribute.className);
                        if (classType == null)
                        {
                            Debug.LogError($"Hook失败，找不到对应class，className:{attribute.className} assemblyName:{attribute.assembleName}");
                            continue;
                        }
                        if (attribute is MonoHookMethodAttribute hookMethodAttribute)
                        {
                            var methodSource = hookMethodAttribute.paramTypes != null
                                ? classType.GetMethod(hookMethodAttribute.methodName, hookMethodAttribute.bindingFlags,
                                    null, hookMethodAttribute.paramTypes, null)
                                : classType.GetMethod(hookMethodAttribute.methodName, hookMethodAttribute.bindingFlags);
                            if (methodSource == null)
                            {
                                Debug.LogError($"Hook失败，找不到对应method，methodName:{hookMethodAttribute.methodName}");
                                continue;
                            }

                            if (hookMethodAttribute.isNative)
                            {
                                var detour = new NativeDetour(methodSource, methodTarget);
                                iDeours.Add(detour);
                                methodName2NativeDetour.Add(methodTarget.Name, detour);
                            }
                            else
                            {
                                iDeours.Add(new Hook(methodSource, methodTarget));
                            }
                        }
                        else if (attribute is MonoHookPropertyAttribute hookPropertyAttribute)
                        {
                            var propertySource = classType.GetProperty(hookPropertyAttribute.propertyName, hookPropertyAttribute.bindingFlags);
                            if (propertySource == null)
                            {
                                Debug.LogError($"Hook失败，找不到对应property，propertyName:{hookPropertyAttribute.propertyName}");
                                continue;
                            }
                        
                            MethodInfo methodSource = hookPropertyAttribute.hookType switch
                            {
                                MonoHookPropertyAttribute.HookType.GetMethod => propertySource.GetGetMethod(),
                                MonoHookPropertyAttribute.HookType.SetMethod => propertySource.GetSetMethod(),
                                _ => null
                            };
                            if (methodSource == null)
                            {
                                Debug.LogError($"Hook失败，property不存在对应GetSet方法，propertyName:{hookPropertyAttribute.propertyName} " +
                                               $"GetSetType:{hookPropertyAttribute.hookType}");
                                continue;
                            }
                            if (hookPropertyAttribute.isNative)
                            {
                                var detour = new NativeDetour(methodSource, methodTarget);
                                iDeours.Add(detour);
                                methodName2NativeDetour.Add(methodTarget.Name, detour);
                            }
                            else
                            {
                                iDeours.Add(new Hook(methodSource, methodTarget));
                            }
                        }
                    }
                }
            }
            s_iDetours = iDeours.ToArray();
        }
    }
}