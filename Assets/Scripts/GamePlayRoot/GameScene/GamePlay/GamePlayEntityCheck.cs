using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using GameBase.Log;
using Cysharp.Text;
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
            var openType = typeof(GamePlaySpawnPoint<>);
            var childrenItem = transform.GetComponentsInChildren<GamePlayEntity>();
            foreach(var child in childrenItem)
            {
                var childType = child.GetType();
                bool isValid = type.IsAssignableFrom(childType);
                if(typeName == typeof(GamePlaySpawnPoint).Name) 
                {
                    if (!isValid && type.IsGenericTypeDefinition)
                    {
                        isValid = IsSubclassOfOpenGeneric(childType, type);
                    }
                }

                if (!isValid)
                {
                    LogUtils.Error(ZString.Concat(label, "下挂有非", typeName, "类且非其子类的节点", child.name, "(", child.GamePlayId, ")","，请检查!!!"), LogChannel.Common, c);
                }
            }
        }
    }

    private bool IsSubclassOfOpenGeneric(Type candidate, Type openGeneric)
    {
        while (candidate != null && candidate != typeof(object))
        {
            var cur = candidate.IsGenericType ? candidate.GetGenericTypeDefinition() : candidate;
            if (cur == openGeneric)
                return true;
            candidate = candidate.BaseType;
        }
        return false;
    }

    public bool GetHierarchyComment(out string name, out Color color)
    {
        name = label;
        color = c;
        return true;
    }
#endif
}
