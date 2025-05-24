using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GameBase.ObjectPool;
using GameBase.Reflection;
using Tools;

namespace GameBase.Utils
{
    public static class ReflectionUtils
    {
        public const BindingFlags ALL_BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Static |
                                                       BindingFlags.Public | BindingFlags.NonPublic;
        //遍历一个类所有变量生成log
        public static string GetFieldsLog<T>(T target)
        {
            var sb = StaticPool<StringBuilder>.Get();
            GetFieldsLog<T>(target, sb);
            var result = sb.ToString();
            StaticPool<StringBuilder>.Return(sb);
            return result;
        }
        
        public static void GetFieldsLog<T>(T target, StringBuilder sb)
        {
            object targetObj = target;
            var type = targetObj.GetType();
            if (type.IsClass || type.IsStruct())
            {
                var fields = type.GetFields(ALL_BINDING_FLAGS);
                sb.Append('[');
                for (var i = 0; i < fields.Length; i++)
                {
                    if(i > 0) sb.Append(' ');
                    var field = fields[i];
                    sb.Append(field.Name);
                    sb.Append(":");
                    sb.Append(field.GetValue(targetObj));
                }
                sb.Append(']');
            }
            else
            {
                sb.Append(targetObj);
            }
        }

        //通过反射调用方法
        public static object InvokeMethod(object sourceObj, string methodName, BindingFlags bindingFlags = ALL_BINDING_FLAGS, object[] _params = null)
        {
            return sourceObj.GetType().GetMethod(methodName, ALL_BINDING_FLAGS).Invoke(sourceObj, _params);
        }
        
        public static object GetField(object sourceObj, string fieldName, BindingFlags bindingFlags = ALL_BINDING_FLAGS)
        {
            return sourceObj.GetType().GetField(fieldName, ALL_BINDING_FLAGS).GetValue(sourceObj);
        }
        
        public static void SetField(object sourceObj, string fieldName, object value, BindingFlags bindingFlags = ALL_BINDING_FLAGS)
        {
            sourceObj.GetType().GetField(fieldName, ALL_BINDING_FLAGS).SetValue(sourceObj, value);
        }
        
        public static object GetProperty(object sourceObj, string fieldName, BindingFlags bindingFlags = ALL_BINDING_FLAGS)
        {
            return sourceObj.GetType().GetProperty(fieldName, ALL_BINDING_FLAGS).GetValue(sourceObj);
        }
        
        public static void SetProperty(object sourceObj, string fieldName, object value, BindingFlags bindingFlags = ALL_BINDING_FLAGS)
        {
            sourceObj.GetType().GetProperty(fieldName, ALL_BINDING_FLAGS).SetValue(sourceObj, value);
        }
        
        public static void SetProperty(object sourceObj, string fieldName, object value, Type returnType, BindingFlags bindingFlags = ALL_BINDING_FLAGS)
        {
            sourceObj.GetType().GetProperty(fieldName, returnType).SetValue(sourceObj, value);
        }
        
#if UNITY_EDITOR

        //将一个类的public成员变量按命名拷贝到另一个类上
        public static void CopyPublicFieldsTo(object source, object target)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
            var fields = source.GetType().GetFields(bindingFlags);
            var targetType = target.GetType();
            foreach (var field in fields)
            {
                var targetField = targetType.GetField(field.Name);
                if (targetField != null)
                {
                    targetField.SetValue(target, field.GetValue(source));
                }
            }
        }

        public static T ShadowCopy<T>(T obj) where T : class
        {
            return InvokeMethod(obj, "MemberwiseClone") as T;
        }
        
        private const BindingFlags DEFAULT_FLAGS =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        //建议用ReflectionManager.GetEditorMethodAttrs，一般来说估计用不到这个
        //assemblyName 为空时表示遍历全部
        public static IEnumerable<(MethodInfo methodInfo, Type type, T attr)> GetAllAssemblyMethodAttribute<T>(
            BindingFlags flags = DEFAULT_FLAGS, string assemblyName = "Assembly-CSharp") where T : Attribute
        {
            foreach (var pair in GetAllAssemblyMethodAttribute(typeof(T), flags, assemblyName))
            {
                yield return (pair.methodInfo, pair.type, pair.attr as T);
            }
        }
        
        //建议用ReflectionManager.GetEditorMethodAttrs，一般来说估计用不到这个
        public static IEnumerable<(MethodInfo methodInfo,Type type,Attribute attr)> GetAllAssemblyMethodAttribute(Type attrType, BindingFlags flags = DEFAULT_FLAGS,string assemblyName = "Assembly-CSharp")
        {
            bool isAssemblyNameEmpty = string.IsNullOrEmpty(assemblyName);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (isAssemblyNameEmpty || assembly.GetName().Name == assemblyName)
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        var methods = type.GetMethods(flags);
                        
                        foreach (var method in methods)
                        {
                            var attr = method.GetCustomAttribute(attrType);
                            if (attr != null)
                            {
                                yield return (method, type, attr);
                            }
                        }
                    }
                }
            }
        }

        public static void SendEditorMethodMsg(string msgName, params object[] parameters)
        {
            var attSet = ReflectionManager.GetEditorMethodAttrs<CallByEditorMsgAttribute>();
            if (attSet != null)
            {
                foreach (var info in attSet)
                {
                    if(info.attribute is CallByEditorMsgAttribute attr)
                    {
                        if (attr.msgName == msgName)
                        {
                            info.methodInfo.Invoke(null, parameters);
                        }
                    }
                }
            }
        }
#endif
    }
}