using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;

public class MonoComment : MonoBehaviour, ICustomHierarchyComment
{
#if UNITY_EDITOR
    public string comment;
    public Color c;
    public bool show;


    public bool GetHierarchyComment(out string name, out Color color)
    {
        name = comment;
        color = c;

        return show;
    }
#endif
}
