using Sirenix.OdinInspector;
using Tools;
using UnityEngine;

public abstract class GamePlayEntity : MonoBehaviour, ICustomHierarchyComment
{
    [ReadOnly]
    public uint GamePlayId;
    [ReadOnly]
    public bool isGen;

    public virtual void Init() { }

#if UNITY_EDITOR
    public abstract bool GetHierarchyComment(out string name, out Color color);
#endif
}
