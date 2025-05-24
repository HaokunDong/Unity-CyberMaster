using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.Utils
{
    public static class ParseEnumCache<T> where T : Enum
    {
        private static Dictionary<string, T> s_cache;

        //带全局缓存的通用string转枚举方法，用于提高string转enum的性能
        //开销省20-30倍（enum约30项），GC为0
        public static T Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                Debug.LogError($"ParseEnumCache转 {typeof(T)} 类型,空值失败");
                return default;
            }

            if (s_cache == null)
            {
                s_cache = new Dictionary<string, T>();
                foreach (var var in Enum.GetValues(typeof(T)))
                {
                    s_cache[var.ToString()] = (T)var;
                }
            }

            if (s_cache.TryGetValue(str, out T res))
            {
                return res;
            }
            else
            {
                Debug.LogError($"ParseEnumCache转 {typeof(T)} 类型,值: {str} 失败");
                return default;
            }
        }
        
        //例子
        // enum TestEnum
        // {
        //     A,
        //     B,
        // }
        // static void Test()
        // {
        //     TestEnum value = ParseEnumCache<TestEnum>.Parse("A");
        // }
    }
}