using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameBase.ObjectPool;

namespace Everlasting.Extend
{
    public static class LinqEx
    {
        public static HashSet<T> ToHashSetEx<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> enumerable)
        {
            return new SortedSet<T>(enumerable);
        }

        public static SortedDictionary<K, V> ToSortedDictionary<K, V>(this IDictionary<K, V> enumerable)
        {
            return new SortedDictionary<K, V>(enumerable);
        }
        
        //打印list所有子节点，调试用
        public static string ToStringChildren(this IEnumerable list)
        {
            var sb = StaticPool<StringBuilder>.Get();
            sb.Append(list);
            sb.Append(" { ");
            int count = 0;
            foreach (var item in list)
            {
                sb.Append(item);
                sb.Append(' ');
                count++;
            }
            sb.Append('}');
            sb.Append(" Count=");
            sb.Append(count);

            var result = sb.ToString();
            StaticPool<StringBuilder>.Return(sb);
            return result;
        }
        
        public static string ToStringChildren<T>(this IEnumerable<T> list)
        {
            var sb = StaticPool<StringBuilder>.Get();
            sb.Append(list);
            sb.Append(" { ");
            int count = 0;
            foreach (var item in list)
            {
                sb.Append(item);
                sb.Append(' ');
                count++;
            }
            sb.Append('}');
            sb.Append(" Count=");
            sb.Append(count);

            var result = sb.ToString();
            StaticPool<StringBuilder>.Return(sb);
            return result;
        }
    }
}