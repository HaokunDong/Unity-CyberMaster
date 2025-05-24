using System.Collections.Generic;

namespace GameBase.ObjectPool
{
    public static class DictionaryPool<K,V>
    {
        private static readonly Stack<Dictionary<K,V>> freeList = new Stack<Dictionary<K,V>>();

        public static Dictionary<K,V> Fetch()
        {
            if (freeList.Count != 0)
            {
                Dictionary<K,V> result = freeList.Pop();
                return result;
            }
            else
            {
                return new Dictionary<K,V>();
            }
        }

        public static void Release(Dictionary<K,V> dic)
        {
            dic.Clear();
            freeList.Push(dic);
        }
    }
}