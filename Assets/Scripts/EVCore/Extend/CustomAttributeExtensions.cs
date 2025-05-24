using System;
using System.Reflection;

namespace Everlasting.Extend
{
    public static class CustomAttributeExtensions
    {
        public static bool TryGetCustomAttribute<T>(this MemberInfo element, out T attribute) where T : Attribute
        {
            attribute = (T) element.GetCustomAttribute(typeof(T));
            return attribute != null;
        }
    }
}