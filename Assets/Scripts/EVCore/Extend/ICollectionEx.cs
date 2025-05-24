using System.Collections.Generic;

namespace Everlasting.Extend
{
    public static class HashSetEx
    {
        public static void AddRange<T>(this ICollection<T> self, IEnumerable<T> elements)
        {
            foreach (var element in elements)
            {
                self.Add(element);
            }
        }
    }
}