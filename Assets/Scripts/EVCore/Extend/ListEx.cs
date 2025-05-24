using System;
using System.Collections.Generic;
using System.Linq;

namespace Everlasting.Extend
{
    public static class ListEx
    {
        private static Random m_rnd = new Random();
        
        public static void Resize<T>(this List<T> list, int size, T element = default(T))
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity) // Optimization
                    list.Capacity = size;

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }
        
        public static int IndexOf<T>(this IList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static T FindFirst<T>(this IList<T> list, Predicate<T> predicate) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    return list[i];
                }
            }
            return null;
        }

        public static void AddIfNotContains<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
        
        public static T RandomGet<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                return list[m_rnd.Next(0, list.Count)];
            }

            return default(T);
        }

        public static void RemoveLast<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }
    }
}