using UnityEngine;

namespace Tools
{
    public interface ICustomHierarchyComment
    {
#if UNITY_EDITOR
        bool GetHierarchyComment(out string name, out Color color);
#endif
    }
}