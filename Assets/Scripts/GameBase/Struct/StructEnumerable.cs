using System.Collections;

namespace GameBase
{
    //用泛型加容器代替了原来的返回接口，避免迭代器struct转接口的GC开销
    public readonly struct StructEnumerable<TEnumerator> where TEnumerator : IEnumerator
    {
        private readonly TEnumerator m_enumerator;

        public StructEnumerable(TEnumerator enumerator)
        {
            m_enumerator = enumerator;
        }
            
        public TEnumerator GetEnumerator()
        {
            return m_enumerator;
        }
    }
    
    //例子
    // private StructEnumerable<HashSet<int>.Enumerator> GetCustomEnumerable()
    // {
    //     data = new HashSet<int>
    //     {
    //         2,
    //         3
    //     };
    //     return new StructEnumerable<HashSet<int>.Enumerator>(data.GetEnumerator());
    // }
    
    // private void Update()
    // {
    //     var enumerable = GetCustomEnumerable();
    //     
    //     foreach (var i in enumerable)
    //     {
    //                 
    //     }
    // }
}