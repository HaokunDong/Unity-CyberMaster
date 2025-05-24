using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Everlasting.Extend
{
    public static class ArrayEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this T[] array, T value)
        {
            return Array.IndexOf(array, value) >= 0;
        }

        public static T[] Add<T>(this T[] array, T value)
        {
            int length = array?.Length ?? 0;
            T[] newArray = new T[length + 1];
            array?.CopyTo(newArray, 0);
            newArray[length] = value;
            return newArray;
        }

        public static T[] Remove<T>(this T[] array, T value)
        {
            if (array == null)
                return null;
            int index = Array.IndexOf(array, value);
            if (index < 0)
                return array;
            IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
            if (array.Length == 1 && comparer.Equals(array[0], value))
                return null;
            int length = array.Length;
            T[] newArray = new T[length - 1];
            Array.Copy(array, 0, newArray, 0, index);
            Array.Copy(array, index + 1, newArray, index, length - index - 1);
            return newArray;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this T[] arr, T value)
        {
            return Array.IndexOf(arr, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this T[] arr, T value, int startIndex)
        {
            return Array.IndexOf(arr, value, startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this T[] arr, T value, int startIndex, int count)
        {
            return Array.IndexOf(arr, value, startIndex, count);
        }
        
        //加where由于非class无法使用==，Equals可能会有潜在的装箱问题
        public static bool IsElementsSame<T>(this T[] arr, T[] arr2) where T : class
        {
            if (arr == null || arr2 == null) return false;
            if (arr.Length != arr2.Length) return false;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != arr2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}