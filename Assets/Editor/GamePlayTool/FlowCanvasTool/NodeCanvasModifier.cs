//using AI.DialogueTrees.CustomGraph;
using GameScene.FlowNode;
using NodeCanvas.Editor;
using UnityEditor;

namespace GamePlayTool.Editor.FlowCanvasTool
{
    public static class NodeCanvasModifier
    {
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            BlackboardEditor.isContextObjectGPGraph = BlackBoardEditorMod;
        }

        private static bool BlackBoardEditorMod(UnityEngine.Object contextObject)
        {
            return contextObject is GamePlayFlowGraph;// || contextObject is CustomDialogueTree;
        }
    }
}