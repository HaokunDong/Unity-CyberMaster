using System.Collections.Generic;

namespace Tools
{
    public interface ICustomHierarchy
    {
#if UNITY_EDITOR
        string Name { get; }
        IEnumerable<object> GetChildren();
#endif
    }
}