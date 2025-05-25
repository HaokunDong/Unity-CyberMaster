#if UNITY_EDITOR

using ParadoxNotion.Design;
using NodeCanvas.Framework;
using UnityEditor;
using UnityEngine;

namespace NodeCanvas.Editor
{

    public class ExternalInspectorWindow : EditorWindow
    {

        private Vector2 scrollPos;
        private bool willRepaint;

        //gx:添加Odin支持，释放时Dispose对应PropertyTree
        private Node m_lastSelectNode;
        
        public static void ShowWindow() {
            var window = GetWindow<ExternalInspectorWindow>();
            window.Show();
        }

        void OnEnable() {
            Prefs.useExternalInspector = true;
            titleContent = new GUIContent("Inspector", StyleSheet.canvasIcon);
            GraphEditorUtility.onActiveElementChanged -= OnActiveElementChange;
            GraphEditorUtility.onActiveElementChanged += OnActiveElementChange;
        }

        void OnDisable() {
            Prefs.useExternalInspector = false;
            GraphEditorUtility.onActiveElementChanged -= OnActiveElementChange;
            if (m_lastSelectNode != null)
            {
                m_lastSelectNode.TryDisposePropertyTree();
                m_lastSelectNode = null;
            }
        }

        void OnActiveElementChange(IGraphElement element) {
            willRepaint = true;
        }

        void Update() {
            if ( willRepaint ) {
                willRepaint = false;
                Repaint();
            }
        }

        void OnGUI() {

            if ( GraphEditor.current == null || GraphEditor.currentGraph == null ) {
                GUILayout.Label("No graph is open in the Graph Editor");
                return;
            }

            if ( EditorApplication.isCompiling && !Application.isPlaying) {
                ShowNotification(new GUIContent("...Compiling Please Wait..."));
                return;
            }

            var currentSelection = GraphEditorUtility.activeElement;
            if (m_lastSelectNode != null && m_lastSelectNode != currentSelection)
            {
                m_lastSelectNode.TryDisposePropertyTree();
                m_lastSelectNode = null;
            }
            
            if ( currentSelection == null ) {
                GUILayout.Label("No selection in Graph Editor");
                return;
            }

            UndoUtility.CheckUndo(currentSelection.graph, "Inspector Change");
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            {
                if ( currentSelection is Node node) {
                    m_lastSelectNode = node;
                    Title(node.name);
                    Node.ShowNodeInspectorGUI(node);
                }

                if ( currentSelection is Connection connection) {
                    Title("Connection");
                    Connection.ShowConnectionInspectorGUI(connection);
                }
            }
            EditorUtils.EndOfInspector();
            GUILayout.EndScrollView();

            UndoUtility.CheckDirty(currentSelection.graph);

            if ( GUI.changed ) {
                GraphEditor.current.Repaint();
            }
        }

        void Title(string text) {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal("box", GUILayout.Height(28));
            GUILayout.FlexibleSpace();
            GUILayout.Label("<b><size=16>" + text + "</size></b>");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorUtils.BoldSeparator();
        }
    }
}

#endif