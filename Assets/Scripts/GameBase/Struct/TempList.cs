using System;
using System.Collections;
using System.Collections.Generic;

namespace Everlasting.Base
{
    public readonly struct TempList<T> : IReadOnlyList<T>, IDisposable
    {
        public readonly List<T> List;

        public TempList(bool init = false)
        {
            List = ListPool<T>.Fetch();
        }
        public void Dispose()
        {
            ListPool<T>.Release(List);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        public int Count => List.Count;

        public T this[int index] => List[index];
    }
}