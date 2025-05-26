using Sirenix.OdinInspector;
using System;
using Tools;
using UnityEngine;

public abstract class GamePlayEntity : MonoBehaviour, ICustomHierarchyComment
{
    [ReadOnly]
    public uint GamePlayId;
    [NonSerialized, ReadOnly, ShowInInspector]
    public bool isGen;
    [NonSerialized, ReadOnly, ShowInInspector]
    public uint TableId;

    [NonSerialized, ReadOnly, ShowInInspector]
    public GamePlaySpawnPoint spawnPoint; 

    public virtual void Init() { }

#if UNITY_EDITOR
    public abstract bool GetHierarchyComment(out string name, out Color color);
#endif
}
