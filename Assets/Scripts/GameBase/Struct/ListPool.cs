using System.Collections.Generic;

namespace Everlasting.Base
{
    public static class ListPool<T>
    {
        private static readonly List<List<T>> freeList = new List<List<T>>();

        public static List<T> Fetch()
        {
            if (freeList.Count != 0)
            {
                List<T> result = freeList[freeList.Count - 1];
                freeList.RemoveAt(freeList.Count - 1);
                return result;
            }
            else
            {
                return new List<T>();
            }
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            freeList.Add(list);
        }
    }
}