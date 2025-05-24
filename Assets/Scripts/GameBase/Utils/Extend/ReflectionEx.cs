using System;

namespace Tools
{
    public static class ReflectionEx
    {
        public static bool IsStruct(this Type type)
        {
            return type.IsValueType && !type.IsEnum && !type.IsPrimitive;
        }
    }
}