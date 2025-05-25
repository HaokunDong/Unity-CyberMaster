using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using GameBase.Log;
using Cysharp.Text;


#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif
using UnityEngine;

public class GamePlayEntityCheck : MonoBehaviour, ICustomHierarchyComment
{
#if UNITY_EDITOR
    [SerializeField]
    private string label;
    [SerializeField] 
    private string typeName;
    [SerializeField]
    private Color c;

    public System.Type type
    {
        get => string.IsNullOrEmpty(typeName) ? null : System.Type.GetType(typeName);
        set => typeName = value?.AssemblyQualifiedName;
    }

    public void Check()
    {
        if (type != null)
        {
            var childrenItem = transform.GetComponentsInChildren<GamePlayEntity>();
            foreach(var child in childrenItem)
            {
                if (!child.GetType().IsAssignableFrom(type))
                {
                    LogUtils.Error(ZString.Concat(label, "下挂有非", typeName, "类型的子节点", child.name, "，请检查!!!"), LogChannel.Common, Color.red);
                }
            }
        }
    }

    public bool GetHierarchyComment(out string name, out Color color)
    {
        name = label;
        color = c;
        return true;
    }
#endif
}
