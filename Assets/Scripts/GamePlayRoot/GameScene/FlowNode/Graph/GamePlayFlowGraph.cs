using System.Collections.Generic;
using System.Linq;
using FlowCanvas;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;

namespace GameScene.FlowNode
{
    public class GamePlayFlowGraph : FlowScriptBase
    {
#if UNITY_EDITOR
        protected override UnityEditor.GenericMenu OnNodesContextMenu(UnityEditor.GenericMenu menu, Node[] nodes) {
            //修改自FlowGraphExtensions.ConvertNodesToMacro，换成了自己类型
            menu.AddItem(new GUIContent("Convert To Macro"), false, () => { ConvertNodesToMacro(nodes.ToList()); });
            return menu;
        }
        
        private void ConvertNodesToMacro(List<Node> originalNodes) {

            if ( originalNodes == null || originalNodes.Count == 0 ) {
                return;
            }

            if ( !UnityEditor.EditorUtility.DisplayDialog("Convert to Macro", "This will create a new Macro out of the nodes.\nPlease note that since Macros are assets, Scene Object references will not be saved.\nThe Macro can NOT be unpacked later on.\nContinue?", "Yes", "No!") ) {
                return;
            }

            //create asset
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Asset of type GamePlayMacro",
                name + "_macro.asset",
                "asset", "", @"Assets\Res\ScriptableObjects\GamePlay\Macro");
            var newMacro = EditorUtils.CreateAsset<GamePlayMacro>(path);
            if ( newMacro == null ) {
                return;
            }

            //undo
            var graph = (FlowScriptBase)originalNodes[0].graph;
            UndoUtility.RecordObjectComplete(graph, "Convert To Macro");

            //clone nodes
            var cloned = Graph.CloneNodes(originalNodes, newMacro, -newMacro.translation);

            //cache used ports
            var inputMergeMapSource = new Dictionary<Port, Port>();
            var inputMergeMapTarget = new Dictionary<Port, Port>();

            var outputMergeMapTarget = new Dictionary<Port, Port>();
            var outputMergeMapSource = new Dictionary<Port, Port>();


            //relink copied nodes to inside macro entry/exit
            for ( var i = 0; i < originalNodes.Count; i++ ) {
                var originalNode = originalNodes[i];
                //create macro entry node port definitions and link those to input ports of cloned nodes inside
                foreach ( var originalInputConnection in originalNode.inConnections.OfType<BinderConnection>() ) {
                    //only do stuff if link source node is not part of the clones
                    if ( originalNodes.Contains(originalInputConnection.sourceNode) ) {
                        continue;
                    }
                    Port defSourcePort = null;
                    //merge same input ports and same target ports
                    if ( !inputMergeMapSource.TryGetValue(originalInputConnection.sourcePort, out defSourcePort) ) {
                        if ( !inputMergeMapTarget.TryGetValue(originalInputConnection.targetPort, out defSourcePort) ) {
                            //remark: we use sourcePort.type instead of target port type, so that connections remain assignable
                            var def = new DynamicParameterDefinition(originalInputConnection.targetPort.name, originalInputConnection.sourcePort.type);
                            defSourcePort = newMacro.AddInputDefinition(def);
                            inputMergeMapTarget[originalInputConnection.targetPort] = defSourcePort;
                        }
                        inputMergeMapSource[originalInputConnection.sourcePort] = defSourcePort;
                    }

                    if ( defSourcePort.CanAcceptConnections() ) { //check this for case of merged FlowPorts
                        var targetPort = ( cloned[i] as FlowCanvas.FlowNode ).GetInputPort(originalInputConnection.targetPortID);
                        BinderConnection.Create(defSourcePort, targetPort);
                    }
                }

                //create macro exit node port definitions and link those to output ports of cloned nodes inside
                foreach ( var originalOutputConnection in originalNode.outConnections.OfType<BinderConnection>() ) {
                    //only do stuff if link target node is not part of the clones
                    if ( originalNodes.Contains(originalOutputConnection.targetNode) ) {
                        continue;
                    }
                    Port defTargetPort = null;
                    //merge same input ports and same target ports
                    if ( !outputMergeMapTarget.TryGetValue(originalOutputConnection.targetPort, out defTargetPort) ) {
                        if ( !outputMergeMapSource.TryGetValue(originalOutputConnection.sourcePort, out defTargetPort) ) {
                            var def = new DynamicParameterDefinition(originalOutputConnection.sourcePort.name, originalOutputConnection.sourcePort.type);
                            defTargetPort = newMacro.AddOutputDefinition(def);
                            outputMergeMapSource[originalOutputConnection.sourcePort] = defTargetPort;
                        }
                        outputMergeMapTarget[originalOutputConnection.targetPort] = defTargetPort;
                    }

                    if ( defTargetPort.CanAcceptConnections() ) { //check this for case of merged ValuePorts
                        var sourcePort = ( cloned[i] as FlowCanvas.FlowNode ).GetOutputPort(originalOutputConnection.sourcePortID);
                        BinderConnection.Create(sourcePort, defTargetPort);
                    }
                }
            }

            //Delete originals
            var originalBounds = RectUtils.GetBoundRect(originalNodes.Select(n => n.rect).ToArray());
            foreach ( var node in originalNodes.ToArray() ) {
                graph.RemoveNode(node, false);
            }

            //Create MacroWrapper. Relink macro wrapper to outside nodes
            var wrapperPos = originalBounds.center;
            wrapperPos.x = (int)wrapperPos.x;
            wrapperPos.y = (int)wrapperPos.y;
            var wrapper = graph.AddMacroNode(newMacro, wrapperPos, null, null);
            wrapper.GatherPorts();
            foreach ( var pair in inputMergeMapSource ) {
                var source = pair.Key;
                var target = wrapper.GetInputPort(pair.Value.ID);
                BinderConnection.Create(source, target);
            }
            foreach ( var pair in outputMergeMapTarget ) {
                var source = wrapper.GetOutputPort(pair.Value.ID);
                var target = pair.Key;
                BinderConnection.Create(source, target);
            }

            //organize a bit
            var clonedBounds = RectUtils.GetBoundRect(cloned.Select(n => n.rect).ToArray());
            newMacro.entry.position = new Vector2((int)( clonedBounds.xMin - 300 ), (int)clonedBounds.yMin);
            newMacro.exit.position = new Vector2((int)( clonedBounds.xMax + 300 ), (int)clonedBounds.yMin);
            newMacro.translation = -newMacro.entry.position + new Vector2(300, 300);

            //dirty
            UndoUtility.SetDirty(graph);
            UndoUtility.SetDirty(newMacro);

            //AddNodeMenu需要重新刷新
            FlowGraphExtensions.ClearCacheMenu();
            
            //validate and save
            newMacro.Validate();
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}