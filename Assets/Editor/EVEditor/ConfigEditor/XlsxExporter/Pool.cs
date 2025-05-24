using System.Collections;
using System.Collections.Generic;

namespace EverlastingEditor.Config.Export
{
    public class Pool : IEnumerable<IPooledData>
    {
        private readonly List<IPooledData> list = new List<IPooledData>();
        private readonly Dictionary<IPooledData, int> dictionary = new Dictionary<IPooledData, int>();

        public IEnumerator<IPooledData> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int GetIndex(IPooledData t)
        {
            int index;
            if(dictionary.TryGetValue(t, out index))
            {
                return index;
            }
            index = list.Count;
            list.Add(t);
            dictionary.Add(t, index);
            return index;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

	    public int Count => list.Count;
    }
}
