using System;
using System.Collections.Generic;
using System.Reflection;
using Everlasting.Extend;

namespace Everlasting.Reflection
{
    public class ReflectionCache
    {
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> propertyPublicCache;
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> propertyPrivateCache;
        private static Dictionary<Type, Dictionary<string, FieldInfo>> fieldPublicCache;
        private static Dictionary<Type, Dictionary<string, FieldInfo>> fieldPrivateCache;
        private static Dictionary<Type, Dictionary<string, MethodInfo>> methodPublicCache;
        private static Dictionary<Type, Dictionary<string, MethodInfo>> methodPrivateCache;

        public static Dictionary<string, PropertyInfo> GetPublicPropertyInfos(Type type)
        {
            if (propertyPublicCache == null)
                propertyPublicCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
            if (!propertyPublicCache.TryGetValue(type, out var properties))
            {
                properties = type.GetProperties(
                    BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.GetProperty |
                    BindingFlags.SetProperty).ToDictionaryIgnoreSameName(p => p.Name);

                propertyPublicCache.Add(type, properties);
            }

            return properties;
        }

        public static Dictionary<string, PropertyInfo> GetPrivatePropertyInfos(Type type)
        {
            if (propertyPrivateCache == null)
                propertyPrivateCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
            if (!propertyPrivateCache.TryGetValue(type, out var properties))
            {
                properties = type.GetProperties(
                    BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.GetProperty |
                    BindingFlags.SetProperty).ToDictionaryIgnoreSameName(p => p.Name);

                propertyPrivateCache.Add(type, properties);
            }

            return properties;
        }

        public static Dictionary<string, FieldInfo> GetPublicFieldInfos(Type type)
        {
            if (fieldPublicCache == null)
                fieldPublicCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
            if (!fieldPublicCache.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                    .ToDictionaryIgnoreSameName(f => f.Name);

                fieldPublicCache.Add(type, fields);
            }

            return fields;
        }

        public static Dictionary<string, FieldInfo> GetPrivateFieldInfos(Type type)
        {
            if (fieldPrivateCache == null)
                fieldPrivateCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
            if (!fieldPrivateCache.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic)
                    .ToDictionaryIgnoreSameName(f => f.Name);

                fieldPrivateCache.Add(type, fields);
            }

            return fields;
        }

        public static Dictionary<string, MethodInfo> GetPublicMethodInfos(Type type)
        {
            if (methodPublicCache == null)
                methodPublicCache = new Dictionary<Type, Dictionary<string, MethodInfo>>();
            if (!methodPublicCache.TryGetValue(type, out var methods))
            {
                methods = type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                    .ToDictionaryIgnoreSameName(f => f.Name);

                methodPublicCache.Add(type, methods);
            }

            return methods;
        }

        
        public static Dictionary<string, MethodInfo> GetPrivateMethodInfos(Type type)
        {
            if (methodPrivateCache == null)
                methodPrivateCache = new Dictionary<Type, Dictionary<string, MethodInfo>>();
            if (!methodPrivateCache.TryGetValue(type, out var methods))
            {
                methods = type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic)
                    .ToDictionaryIgnoreSameName(f => f.Name);
                
                methodPrivateCache.Add(type, methods);
            }

            return methods;
        }
    }
}