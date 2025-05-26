using System;
using System.Collections.Generic;
using Everlasting.Extend;
using FlowCanvas;
using GameBase.Log;
using GameScene.FlowNode;
//using GameScene.FlowNode.Action;
using NodeCanvas.Framework;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Tools;
using Tools.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GamePlayTool.Editor.FlowCanvasTool
{
    //用于寻找某个节点被哪些图使用
    public class FindFlowNodeReferenceWindow : OdinEditorWindow
    {
        [MenuItem("GamePlay/蓝图节点全局搜索")]
        private static void ShowCustomDefaultSceneWindow()
        {
            var window = GetWindow<FindFlowNodeReferenceWindow>();
            window.Show();
        }

        public string searchName;

        [Button]
        private void Search()
        {
            searchResults.Clear();
            
            //遍历asset
            var assetGUIDS = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(Graph).Name));
            foreach ( var guid in assetGUIDS ) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<Graph>(path);
                if (asset)
                {
                    SearchGraph(path, asset, asset.allNodes);
                }
            }

            List<GamePlayFlowController> controllers = new List<GamePlayFlowController>();
            //遍历关卡蓝图
            EditorFileUtils.ForeachPrefab(Application.dataPath + @"/Res/Maps/", SearchBoundGraph);
            //交互物蓝图也有可能有bound模式
            EditorFileUtils.ForeachPrefab(Application.dataPath + @"/Res/Prefabs/SceneItem/", SearchBoundGraph);

            bool SearchBoundGraph(string path, GameObject prefab)
            {
                controllers.Clear();
                EditorUtils.ForeachChildrenDoUntil(prefab, child =>
                {
                    //遇到递归prefab直接返回
                    if(PrefabUtility.IsAnyPrefabInstanceRoot(child) && child != prefab) return true;
                    GamePlayFlowController control = child.GetComponent<GamePlayFlowController>();
                    if (control != null)
                    {
                        controllers.Add(control);
                        return true;
                    }
                    return false;
                });
                if (controllers.Count > 0)
                {
                    // var go = GameObject.Instantiate(prefab);
                    foreach (var gamePlayFlowController in controllers)
                    {
                        var graph = ScriptableObject.CreateInstance<GamePlayFlowGraph>();
                        graph.Deserialize(gamePlayFlowController.boundGraphSerialization, new List<Object>(), false);
                        SearchGraph(EditorFileUtils.SubStringToAssets(path), prefab, graph.allNodes);
                        ScriptableObject.DestroyImmediate(graph);
                    }
                    // GameObject.DestroyImmediate(go);
                }
                //这玩意卡爆
                EditorUtility.UnloadUnusedAssetsImmediate();
                return false;
            }
            
            void SearchGraph(string path, Object asset, List<Node> nodes)
            {
                LogUtils.Debug($"遍历Path：{path}");
                foreach (var node in nodes)
                {
                    if (node.GetType().Name.Contains(searchName))
                    {
                        AddResult(node);
                    }
                    //else if (node is DesignMessageAction designMessageAction)
                    //{
                    //    if (designMessageAction.message.value.Contains(searchName))
                    //    {
                    //        AddResult(node);
                    //    }
                    //}
                    //else if (node is DesignMessageAsyncAction designMessageActionAsync)
                    //{
                    //    if (designMessageActionAsync.message.value.Contains(searchName))
                    //    {
                    //        AddResult(node);
                    //    }
                    //}
                }

                void AddResult(Node node)
                {
                    searchResults.Add(new Result()
                    {
                        path = path,
                        asset = asset,
                        nodeName = node.name,
                    });
                }
            }
        }

        [Serializable]
        public struct Result
        {
            public string path;
            public Object asset;
            public string nodeName;
        }

        [TableList(AlwaysExpanded = true), ReadOnly]
        public List<Result> searchResults = new List<Result>();
    }
}