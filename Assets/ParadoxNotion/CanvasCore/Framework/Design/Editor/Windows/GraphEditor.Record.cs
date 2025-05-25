#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using UnityEditor;
//Griffin 修改NodeCanvas插件和unity版本适配
using UnityEditor.SceneManagement;
//Griffin 修改NodeCanvas插件和unity版本适配
using UnityEngine;
using Object = UnityEngine.Object;

namespace NodeCanvas.Editor
{
    //为GraphEditor增加历史记录跳转功能
    public partial class GraphEditor
    {
        [Serializable]
        internal struct GraphHistoryRecord
        {
            public enum Type
            {
                None, //无法找到历史记录
                Asset,
                InPrefab,
                Scene,
            }

            public Type type;
            public string assetPath; //资源路径
            public string scenePath; //在场景中的路径

            public GraphHistoryRecord(Graph graph)
            {
                type = Type.None;
                assetPath = "";
                scenePath = "";
                if (graph == null)
                {
                    return;
                }

                var owner = graph.agent as GraphOwner;
                if (owner != null)
                {
                    // if (owner.graphIsBound)
                    {
                        var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(owner.gameObject);
                        if (!string.IsNullOrEmpty(prefabPath))
                        {
                            assetPath = prefabPath;
                            scenePath = GetPathUntilParent(owner.gameObject.transform,PrefabUtility.GetNearestPrefabInstanceRoot(owner.gameObject).transform, false);
                            type = Type.InPrefab;
                            return;
                        }
                        else
                        {
                            //检查prefabStage
                            var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                            if (currentPrefabStage != null)
                            {
                                assetPath = currentPrefabStage.assetPath;
                                scenePath = GetPathUntilParent(owner.gameObject.transform,currentPrefabStage.prefabContentsRoot.transform, false);
                                type = Type.InPrefab;
                                return;
                            }
                            else
                            {
                                assetPath = "";
                                scenePath = GetPath(owner.gameObject.transform);
                                type = Type.Scene;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    //纯asset类型
                    assetPath = AssetDatabase.GetAssetPath(graph);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        type = Type.Asset;
                        scenePath = "";
                        return;
                    }
                }
            }

            // public bool IsAvailable()
            // {
            //     return true;
            // }

            //跳转到该记录上
            // public void JumpToThis()
            // {
            //     Debug.LogError(this);
            // }

            public bool Equals(GraphHistoryRecord other)
            {
                return assetPath == other.assetPath && scenePath == other.scenePath;
            }

            public override bool Equals(object obj)
            {
                return obj is GraphHistoryRecord other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((assetPath != null ? assetPath.GetHashCode() : 0) * 397) ^ (scenePath != null ? scenePath.GetHashCode() : 0);
                }
            }

            public override string ToString()
            {
                return $"{nameof(type)}: {type}, {nameof(assetPath)}: {assetPath}, {nameof(scenePath)}: {scenePath}";
            }
        }

        //必须套一层，不然unity不认List
        [Serializable]
        private struct GraphHistoryRecordSave
        {
            public List<GraphHistoryRecord> historyRecords;
        }

        [InitializeOnLoadMethod]
        private static void OnLoadInitialize()
        {
            string saveJson = EditorPrefs.GetString(SAVE_KEY);
            if (!string.IsNullOrEmpty(saveJson))
            {
                var save = JsonUtility.FromJson<GraphHistoryRecordSave>(saveJson);
                s_historyRecords = save.historyRecords;
            }
        }

        private const string SAVE_KEY = "NodeCanvas_History_Save";
        private const int MAX_HISTORY_COUNT = 8;
        private static List<GraphHistoryRecord> s_historyRecords = new List<GraphHistoryRecord>(MAX_HISTORY_COUNT);
    
        private void RecordNewHistory(Graph graph)
        {
            if (graph == null) return;
            var newRecord = new GraphHistoryRecord(graph);
            if (newRecord.type == GraphHistoryRecord.Type.None) return;
            int index;
            if ((index = s_historyRecords.IndexOf(newRecord)) > -1)
            {
                //置顶
                s_historyRecords.RemoveAt(index);
                s_historyRecords.Add(newRecord);
                return;
            }
            if (s_historyRecords.Count >= MAX_HISTORY_COUNT) s_historyRecords.RemoveAt(0);
            s_historyRecords.Add(newRecord);
            var json = JsonUtility.ToJson(new GraphHistoryRecordSave(){historyRecords = s_historyRecords});
            EditorPrefs.SetString(SAVE_KEY, json);
            // Debug.LogError(json);
        }

        private void JumpToHistory(GraphHistoryRecord record)
        {
            int index;
            if ((index = s_historyRecords.IndexOf(record)) > -1)
            {
                //置顶
                s_historyRecords.RemoveAt(index);
                s_historyRecords.Add(record);
            }

            bool findTarget = false;
            if (!string.IsNullOrEmpty(record.assetPath))
            {
                if (record.assetPath.EndsWith(".prefab", StringComparison.Ordinal))
                {
                    var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                    if (prefabStage != null && prefabStage.assetPath == record.assetPath)
                    {
                        var trans = prefabStage.prefabContentsRoot.transform.Find(record.scenePath);
                        if (trans)
                        {
                            Selection.activeObject = trans.gameObject;
                            SetReferences(trans.gameObject.GetComponent<GraphOwner>());
                            findTarget = true;
                        }
                    }
                    else
                    {
                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(record.assetPath);
                        if (go)
                        {
                            AssetDatabase.OpenAsset(go);
                            prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                            if (prefabStage != null)
                            {
                                var trans = prefabStage.prefabContentsRoot.transform.Find(record.scenePath);
                                if (trans)
                                {
                                    Selection.activeObject = trans.gameObject;
                                    SetReferences(trans.gameObject.GetComponent<GraphOwner>());
                                    findTarget = true;
                                }
                            }
                        }
                    }
                }
                else if(record.assetPath.EndsWith(".asset", StringComparison.Ordinal))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Graph>(record.assetPath);
                    if (asset)
                    {
                        Selection.activeObject = asset;
                        SetReferences(asset);
                        findTarget = true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(record.scenePath))
            {
                var go = GameObject.Find(record.scenePath);
                if (go)
                {
                    SetReferences(go.GetComponent<GraphOwner>());
                    Selection.activeObject = go;
                    findTarget = true;
                }
            }

            if (!findTarget)
            {
                Debug.LogError($"GraphHistoryRecord Error {record}");
            }
        }

        private void DrawHistoryMenu()
        {
            var menu = new GenericMenu();
            var currentRecord = new GraphHistoryRecord(currentGraph);
            for (var i = s_historyRecords.Count - 1; i >= 0; i--)
            {
                var record = s_historyRecords[i];
                bool isSame = record.Equals(currentRecord);
                string menuName;
                if (string.IsNullOrEmpty(record.assetPath))
                {
                    var go = GameObject.Find(record.scenePath);
                    if (!go)
                    {
                        s_historyRecords.RemoveAt(i);
                        continue;
                    }

                    menuName = record.scenePath.Replace('/', '\\');
                }
                else
                {
                    menuName = SubTailLastOf(record.assetPath, "/");
                }
                menu.AddItem(new GUIContent(menuName), isSame, () =>
                {
                    if (isSame) return;
                    JumpToHistory(record);
                });
            }

            menu.ShowAsContext();
        }
        
        #region Utils
        private static string GetPath(Transform self)
        {
            var names = new List<string>();
            {
                Transform trans = self;
                do
                {
                    names.Add(trans.gameObject.name);
                    trans = trans.parent;
                } while (trans);

                names.Reverse();

                return string.Join("/", names);
            }
        }
        
        private static string SubTailLastOf(string str, string value)
        {
            int index = str.LastIndexOf(value, StringComparison.Ordinal);
            if (index > -1)
            {
                return index + 1 < str.Length ? str.Substring(index + 1) : "";
            }
            return str;
        }
        
        private static string GetPathUntilParent(Transform self, Transform parent, bool containParentSelf = true)
        {
            var names = new List<string>();
            {
                Transform trans = self;
                do
                {
                    if (trans == parent)
                    {
                        if(containParentSelf) names.Add(trans.gameObject.name);
                        break;
                    }
                    names.Add(trans.gameObject.name);
                    trans = trans.parent;
                } while (trans);
                
                names.Reverse();

                return string.Join("/", names);
            }
        }
        
        #endregion
    }
}
#endif