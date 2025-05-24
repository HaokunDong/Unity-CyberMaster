using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Everlasting.Extend
{
    public static class StringEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format(this string format, object arg0)
        {
            return string.Format(format, arg0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format(this string format, object arg0, object arg1)
        {
            return string.Format(format, arg0, arg1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format(this string format, object arg0, object arg1, object arg2)
        {
            return string.Format(format, arg0, arg1, arg2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] Split(this string str, char separator, StringSplitOptions options)
        {
            return str.Split(new char[] {separator}, options);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] Split(this string str, string separator, StringSplitOptions options)
        {
            return str.Split(new string[] {separator}, options);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string str, string value, StringComparison comparison)
        {
            return str.IndexOf(value, comparison) >= 0;
        }
        
        //StringComparison.Ordinal相比StringComparison.CurrentCulture会有20倍以上的性能差别
        //https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/shared/System/String.Comparison.cs 有兴趣的可以读一下官方源码
        //不建议使用StringComparison.CurrentCulture
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithEx(this string str, string value, StringComparison comparison = StringComparison.Ordinal)
        {
            return str.StartsWith(value, comparison);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWithEx(this string str, string value, StringComparison comparison = StringComparison.Ordinal)
        {
            return str.EndsWith(value, comparison);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string CutStartsWith(this string str, string value)
        {
            if (str.StartsWithEx(value))
            {
                return str.Substring(value.Length);
            }
            return str;
        }
        
        public static string CutEndsWith(this string str, string value)
        {
            if (str.EndsWithEx(value))
            {
                return str.Substring(0,str.Length - value.Length);
            }
            return str;
        }
        
        public static string SubPreFirstOf(this string str, string value)
        {
            int index = str.IndexOf(value, StringComparison.Ordinal);
            if (index > -1)
            {
                return str.Substring(0, index);
            }
            return str;
        }
        
        public static string SubTailFirstOf(this string str, string value)
        {
            int index = str.IndexOf(value, StringComparison.Ordinal);
            if (index > -1)
            {
                return index + 1 < str.Length ? str.Substring(index + value.Length) : "";
            }
            return str;
        }
        
        public static string SubPreLastOf(this string str, string value)
        {
            int index = str.LastIndexOf(value, StringComparison.Ordinal);
            if (index > -1)
            {
                return str.Substring(0, index);
            }
            return str;
        }
        
        public static string SubTailLastOf(this string str, string value)
        {
            int index = str.LastIndexOf(value, StringComparison.Ordinal);
            if (index > -1)
            {
                return index + 1 < str.Length ? str.Substring(index + value.Length) : "";
            }
            return str;
        }

        public static string AddTailFileName(this string str, string tail)
        {
            var ext = Path.GetExtension(str);
            var newPath = $"{str.Substring(0, str.Length - ext.Length)}{tail}{ext}";
            return newPath;
        }
    }
}