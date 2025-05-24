using UnityEditor;
using UnityEngine;
using BehaviourTrees;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

public class SMAndBTGraphWindow : EditorWindow
{
    private SMAndBTGraphView graphView;

    [MenuItem("Tools/状态机和行为树查看")]
    public static void ShowWindow()
    {
        var window = GetWindow<SMAndBTGraphWindow>();
        window.titleContent = new GUIContent("状态机和行为树查看");
        window.Show();
    }

    private void OnEnable()
    {
        graphView = new SMAndBTGraphView();
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
        graphView.EnableAutoRedrawOnResize();
        graphView.StartAutoStateCheck();
        Selection.selectionChanged += OnSelectionChange;
        OnSelectionChange();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
        graphView.StopAutoStateCheck();
        Selection.selectionChanged -= OnSelectionChange;
    }

    private void OnSelectionChange()
    {
        if (Selection.activeGameObject != null)
        {
            var e = Selection.activeGameObject.GetComponent<Enemy>();
            var p = Selection.activeGameObject.GetComponent<Player>();
            if(e != null)
            {
                var dict = StateHelper.GetAllStates<EnemyState>(e);
                graphView.DrawNodesOnEdges(dict, e.stateMachine);
            }
            else if(p != null)
            {
                var dict = StateHelper.GetAllStates<PlayerState>(p);
                graphView.DrawNodesOnEdges(dict, p.stateMachine);
            }
        }
    }

    private BTNode GetBTNodeFromState(EnemyState state)
    {
        var field = state.GetType().GetField("behaviourTree", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(state) as BTNode;
    }
}


public static class StateHelper
{
    public static Dictionary<string, TState> GetAllStates<TState>(object obj) where TState : class
    {
        Dictionary<string, TState> res = new ();
        var type = obj.GetType();  // 用实例的实际类型开始遍历

        while (type != null && type != typeof(object))
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
            {
                if (typeof(TState).IsAssignableFrom(field.FieldType))
                {
                    var state = field.GetValue(obj) as TState;
                    if (state != null)
                    {
                        res[state.ToString()] = state;
                    }
                }
            }

            type = type.BaseType;
        }

        return res;
    }
}

