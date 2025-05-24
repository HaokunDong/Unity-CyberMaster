using GameBase.Log;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class SingletonCompAwakeChecker
{
    [InitializeOnLoadMethod]
    private static void CheckForAwakeInSingletonChildren()
    {
        // 延迟调用以确保类型加载完成
        EditorApplication.delayCall += () =>
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.FullName.StartsWith("Unity"))
                .SelectMany(asm =>
                {
                    try { return asm.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                });

            foreach (var type in allTypes)
            {
                if (!type.IsClass || type.IsAbstract) continue;

                // 检查是否继承自 SingletonComp<T>
                var baseType = type.BaseType;
                while (baseType != null)
                {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(SingletonComp<>))
                        break;

                    baseType = baseType.BaseType;
                }

                if (baseType == null) continue;

                // 查找 Awake 方法（自身定义的）
                var awakeMethod = type.GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                if (awakeMethod != null && awakeMethod.DeclaringType == type)
                {
                    LogUtils.Error(
                        $"类 {type.FullName} 继承自 SingletonComp<T>，但自定义了 Awake() 方法。\n" +
                        $"这可能会绕过 Singleton 的初始化流程，推荐改为重写 OnSingletonInit()。",
                        LogChannel.Common,
                        Color.cyan
                    );
                }
            }
        };
    }
}
