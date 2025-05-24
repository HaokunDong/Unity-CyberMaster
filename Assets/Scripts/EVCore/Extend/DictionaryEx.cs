using System;
using System.Collections.Generic;

namespace Everlasting.Extend
{
    public static class DictionaryEx
    {
        
        public static TValue GetValueOrDefaultEx<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            dict.TryGetValue(key, out var value);

            return value;
        }

        public static TValue GetValueOrDefaultEx<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
            TValue defaultValue)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                return defaultValue;
            }

            return value;
        }
        
        /// <summary>
        /// 往一个Dic(TKey，List(TValue))类型的字典里添加元素
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddKeyListValue<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            List<TValue> listValue = null;
            if (!dictionary.TryGetValue(key, out listValue))
                dictionary[key] = listValue = new List<TValue>();

            listValue.Add(value);
        }

        /// <summary>
        /// 在一个Dic(TKey，List(TValue))类型的字典里删除元素
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void RemoveKeyListValue<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            List<TValue> listValue = null;
            if (dictionary.TryGetValue(key, out listValue))
            {
                if (listValue.Remove(value) && listValue.Count == 0)
                {
                    dictionary.Remove(key);
                }
            }
        }

        /// <summary>
        /// 往一个Dic(TKey，HashSet(TValue))类型的字典里添加元素
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="tKey"></param>
        /// <param name="tValue"></param>
        public static void AddKeyHashSetValue<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> dic, TKey tKey, TValue tValue)
        {
            HashSet<TValue> listValue = null;
            if (!dic.TryGetValue(tKey, out listValue))
                dic[tKey] = listValue = new HashSet<TValue>();

            listValue.Add(tValue);
        }

        /// <summary>
        /// 往一个Dic(TKey，Dic(TValue,T3))类型的字典里添加元素
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="dic"></param>
        /// <param name="tKey1"></param>
        /// <param name="tKey2"></param>
        /// <param name="tValue"></param>
        public static void AddKeyDictSetValue<TKey, TValue, T3>(this IDictionary<TKey, Dictionary<TValue, T3>> dic, TKey tKey1, TValue tKey2, T3 tValue)
        {
            Dictionary<TValue, T3> subDic = null;
            if (!dic.TryGetValue(tKey1, out subDic))
                dic[tKey1] = subDic = new Dictionary<TValue, T3>();

            subDic[tKey2] = tValue;
        }

        /// <summary>
        /// 如果字典内不存在key，则把key赋值为defaultValue，并返回true；如果已经存在该key，则什么也不做，返回false
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="tKey"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool SetDefaultIfNotExists<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey tKey, TValue defaultValue)
        {
            if (dic.ContainsKey(tKey))
                return false;

            dic[tKey] = defaultValue;
            return true;
        }

        /// <summary>
        /// 从字典中获取Key对应的value的引用；如果不存在，则创建一个实例，添加到字典中，并返回它;
        /// TValue 必须为引用类型
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="tKey"></param>
        /// <returns></returns>
        public static TValue GetExistValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey tKey)
            where TValue : class, new()
        {
            TValue t2 = default(TValue);
            if (!dic.TryGetValue(tKey, out t2))
                dic[tKey] = t2 = new TValue();

            return t2;
        }

        public static int RemoveIf<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            List<TKey> toBeRemoved = null;
            foreach (var pair in dic)
            {
                if (predicate(pair))
                {
                    if (toBeRemoved == null)
                        toBeRemoved = new List<TKey>();
                    toBeRemoved.Add(pair.Key);
                }
            }

            if (toBeRemoved == null)
                return 0;

            foreach (var key in toBeRemoved)
            {
                dic.Remove(key);
            }

            return toBeRemoved.Count;
        }

        public static int RemoveIf<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            Predicate<TKey> predicate)
        {
            List<TKey> toBeRemoved = null;
            foreach (var pair in dic)
            {
                if (predicate(pair.Key))
                {
                    if (toBeRemoved == null)
                        toBeRemoved = new List<TKey>();
                    toBeRemoved.Add(pair.Key);
                }
            }

            if (toBeRemoved == null)
                return 0;

            foreach (var key in toBeRemoved)
            {
                dic.Remove(key);
            }

            return toBeRemoved.Count;
        }
        
        public static Dictionary<TKey, TValue> ToDictionaryIgnoreSameName<TKey, TValue>(
            this IEnumerable<TValue> source,
            Func<TValue, TKey> keyGetter)
        {
            return ToDictionaryIgnoreSameName(source, keyGetter, element => element);
        }
        
        public static Dictionary<TKey, TValue> ToDictionaryIgnoreSameName<TElement, TKey, TValue>(
            this IEnumerable<TElement> source,
            Func<TElement, TKey> keyGetter,
            Func<TElement, TValue> valueGetter)
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
            foreach (var e in source)
            {
                var key = keyGetter(e);
                if (dict.ContainsKey(key))
                {
                    continue;
                }

                dict.Add(key, valueGetter(e));
            }
            return dict;
        }
    }
}