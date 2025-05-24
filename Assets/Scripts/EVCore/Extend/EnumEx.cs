using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Everlasting.Extend
{
    public static class EnumEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToUInt32<TEnum>(this TEnum @enum) where TEnum : Enum
        {
            return Unsafe.As<TEnum, uint>(ref @enum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt32<TEnum>(this TEnum @enum) where TEnum : Enum
        {
            return Unsafe.As<TEnum, int>(ref @enum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToUInt32(this Enum @enum)
        {
            return Convert.ToUInt32(@enum, CultureInfo.InvariantCulture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt32(this Enum @enum)
        {
            return Convert.ToInt32(@enum, CultureInfo.InvariantCulture);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlag<TEnum>(this TEnum @enum, TEnum flag) where TEnum : Enum
        {
            return (@enum.ToInt32() & flag.ToInt32()) != 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlag<TEnum>(this TEnum @enum, int flag) where TEnum : Enum
        {
            return (@enum.ToInt32() & flag) != 0;
        }
    }
}