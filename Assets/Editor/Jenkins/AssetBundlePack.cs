using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using AssetLoad;
using Cysharp.Threading.Tasks;
using GameBase.Log;
using Tools;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEditor.SceneManagement;
using Cysharp.Text;

namespace Hypergryph
{
    public class AssetBundlePack
    {
        [Serializable]
        public class Res_Filter_White
        {
            public string path;
            public int filter_flag;
            public string suffix;
            public int one_ab;
        }

        public class AssetBundleNode
        {
            /// <summary>
            /// Asset/xxx/yyy
            /// </summary>
            public string path;
            public bool isRoot;
            public bool isFolder;
            public bool isDelete;
            public List<string> assetPaths = new List<string>();
            public Dictionary<string, AssetBundleNode> childNodeDict = new Dictionary<string, AssetBundleNode>();
            public Dictionary<string, AssetBundleNode> parentNodeDict = new Dictionary<string, AssetBundleNode>();

            public bool CheckLoopNode(AssetBundleNode node)
            {
                bool isLoop = false;
                if (childNodeDict.ContainsKey(node.path))
                {
                    isLoop = true;
                }
                else
                {
                    foreach (var childNode in childNodeDict.Values)
                    {
                        isLoop = childNode.CheckLoopNode(node);
                        if (isLoop)
                        {
                            LogUtils.Error("经过循环节点:" + childNode.path);
                            break;
                        }
                    }
                }
                return isLoop;
            }

            public void CombineNode(AssetBundleNode combineNode)
            {
                foreach (var childNode in childNodeDict.Values)
                {
                    childNode.parentNodeDict.Remove(path);
                    if (!combineNode.childNodeDict.ContainsKey(childNode.path) && combineNode.path != childNode.path)
                    {
                        combineNode.childNodeDict.Add(childNode.path, childNode);
                        childNode.parentNodeDict.Add(combineNode.path, combineNode);
                    }
                }

                foreach (var parentNode in parentNodeDict.Values)
                {
                    parentNode.childNodeDict.Remove(path);
                    if (!combineNode.parentNodeDict.ContainsKey(parentNode.path) && combineNode.path != parentNode.path)
                    {
                        combineNode.parentNodeDict.Add(parentNode.path, parentNode);
                        parentNode.childNodeDict.Add(combineNode.path, combineNode);
                    }
                }
                combineNode.assetPaths.AddRange(assetPaths);
            }


            public bool IsParentNodeEqual(AssetBundleNode compareNode)
            {
                bool isEqual = true;
                if (path != compareNode.path && parentNodeDict.Count > 0 && parentNodeDict.Count == compareNode.parentNodeDict.Count)
                {
                    foreach (var parentPath in parentNodeDict.Keys)
                    {
                        if (!compareNode.parentNodeDict.ContainsKey(parentPath))
                        {
                            isEqual = false;
                            break;
                        }
                    }
                }
                else
                {
                    isEqual = false;
                }

                return isEqual;
            }
        }
        
        public static BuildTarget s_targetPlatform;

        /// 重复时，需要直接终止
        private static List<string> s_buildAssetLowerPaths = new List<string>();
        /// <summary>
        /// 需要打包的节点(ab路径->ab资源节点)
        /// </summary>
        private static Dictionary<string, AssetBundleNode> s_buildAssetBundleNodeDict =
            new Dictionary<string, AssetBundleNode>();

        private static List<Res_Filter_White> _ProcessActivityData(ExcelTable table)
        {
            List<Res_Filter_White> allConfig = new List<Res_Filter_White>();
            int act_col = 0;
            int suffix_col = 0;
            int flag_col = 0;
            int one_ab_col = 0;
            for (int i = 1; i <= table.NumberOfColumns; i++)
            {
                var title = table.GetValue(1, i).ToString();
                if (title.Equals("path"))
                {
                    act_col = i;
                }
                else if (title.Equals("suffix"))
                {
                    suffix_col = i;
                }
                else if (title.Equals("filter_flag"))
                {
                    flag_col = i;
                }
                else if (title.Equals("one_ab"))
                {
                    one_ab_col = i;
                }
            }

            for (int i = 4; i <= table.NumberOfRows; i++)
            {
                string act_id = table.GetValue(i, act_col).ToString();
                if (act_id.Equals(""))
                    continue;

                string filter_flag = table.GetValue(i, flag_col).ToString();
                if (filter_flag != "1")
                    continue;

                string one_ab = table.GetValue(i, one_ab_col).ToString();

                string suffix = table.GetValue(i, suffix_col).ToString();

                Res_Filter_White unit = new Res_Filter_White();
                unit.path = act_id.Replace("\\", "/");
                unit.filter_flag = int.Parse(filter_flag);
                unit.suffix = suffix;
                unit.one_ab = int.Parse(one_ab);

                allConfig.Add(unit);
            }

            return allConfig;
        }

        public static List<Res_Filter_White> GetConfigDatas(ref List<string> blackList)
        {
            var path = ZString.Format("{0}{1}", AssetBundleData.s_projectPath,
                "Tools/EditorExcel/res_filter_config.xlsx");

            Excel excel = ExcelHelper.LoadExcel(path);
            if (excel.Tables.Count <= 0){
                LogUtils.Warning("excel sheet not exists in excel file, path:" + path);
                return null;
            }

            List<Res_Filter_White> datas = null;
            for (int i = 0; i < excel.Tables.Count; i++)
            {
                var table = excel.Tables[i];
                if (table.TableName.Equals("white")){
                    datas = _ProcessActivityData(table);
                }

                if (table.TableName.Equals("black")){
                    blackList = _FilterBlackFiles(table);
                }
            }

            return datas;
        }

        /// <summary>
        /// 将每个子文件夹都单独打成一个ab，主文件夹和零散的子文件都不处理
        /// </summary>
        private static void _ProcessSubFolderAB(ref HashSet<string> processedFiles, List<Res_Filter_White> datas,
            List<string> blackList)
        {
            foreach (var iter in datas)
            {
                if (iter.filter_flag == 0)
                {
                    continue;
                }

                if (iter.one_ab != 2)
                {
                    continue;
                }

                string fullPath = ZString.Concat(AssetBundleData.s_projectPath, iter.path);
                var subFolders = Directory.GetDirectories(fullPath, "*.*", SearchOption.TopDirectoryOnly);
                if (subFolders == null || subFolders.Length == 0)
                {
                    continue;
                }

                foreach (var subIter in subFolders)
                {
                    string assetName = AssetBundleData.AbsolutePathToUnityPath(subIter);
                    _ProcessUnitFolderAB(assetName, assetName, iter.suffix,
                        ref processedFiles, blackList);
                }
            }
        }

        /// <summary>
        /// 以文件夹进行打包时，让文件夹下的所有文件不会被再次单独打成ab
        /// </summary>
        private static void _ProcessFolderAB(ref HashSet<string> processedFiles, List<Res_Filter_White> datas, List<string> blackList)
        {
            foreach (var iter in datas)
            {
                if (iter.filter_flag == 0)
                {
                    continue;
                }

                if (iter.one_ab == 1) //将整个文件夹打包成一个ab
                {
                    string fullPath = ZString.Concat(AssetBundleData.s_projectPath, iter.path);
                    string abName = AssetBundleData.AbsolutePathToUnityPath(fullPath);
                    _ProcessUnitFolderAB(fullPath, abName, iter.suffix, ref processedFiles, blackList);
                }
            }
        }


        //对文件夹进行打包
        private static void _ProcessUnitFolderAB(string fullPath, string folderPath, string suffix,
            ref HashSet<string> processedFiles, List<string> blackList)
        {
            if (SysUtils.DirectoryExists(fullPath))
            {
                string[] files = null;
                if (string.IsNullOrEmpty(suffix))
                {
                    files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);
                }
                else
                {
                    files = Directory.GetFiles(fullPath, ZString.Format("{0}{1}", "*.", suffix),
                        SearchOption.AllDirectories);
                }
                List<string> allAssetList = new List<string>();

                var tempSet = new HashSet<string>();

                for (int i = 0; i < files.Length; i++)
                {
                    string relativePath = AssetBundleData.AbsolutePathToUnityPath(files[i]);
                    if (!_CheckPathLegal(processedFiles, blackList, relativePath))
                    {
                        continue;
                    }

                    Debug.Log(ZString.Format("{0}{1}", "assetName : ", relativePath));
                    tempSet.Add(relativePath);
                    allAssetList.Add(relativePath);
                    processedFiles.Add(relativePath);
                }
                
                if (!s_buildAssetBundleNodeDict.ContainsKey(folderPath))
                {
                    AssetBundleNode assetBundleNode = new AssetBundleNode();
                    assetBundleNode.path = folderPath;
                    assetBundleNode.isRoot = true;
                    assetBundleNode.isFolder = true;
                    s_buildAssetBundleNodeDict.Add(folderPath, assetBundleNode);
                    foreach (var assetPath in allAssetList)
                    {
                        var assetNode = CreateAssetBundleNode(assetPath);
                        if (assetNode != null)
                        {
                            assetBundleNode.childNodeDict.Add(assetPath, assetNode);
                            assetNode.parentNodeDict.Add(assetBundleNode.path, assetBundleNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 对独立的文件进行打包
        /// </summary>
        private static void _ProcessSpecifiedFileAB(ref HashSet<string> processedFiles, List<Res_Filter_White> datas, List<string> blackList)
        {
            foreach (var iter in datas)
            {
                if (iter.filter_flag == 0)
                {
                    continue;
                }

                string fullPath = ZString.Concat(AssetBundleData.s_projectPath, iter.path);
                string relativePath = AssetBundleData.AbsolutePathToUnityPath(fullPath);
                if(FileUtils.FileExistsWithCaseSensitivity(fullPath))
                {
                    _AddRelativePath(ref processedFiles, blackList, relativePath, 
                        "_ProcessSpecifiedFileAB", "lutaian");
                }
            }
        }

        private static bool _CheckAssetNameDuplicate(string path)
        {
            var lower = path.ToLower();
            if (s_buildAssetLowerPaths.Contains(lower))
            {
                var content = $"hyper-- ab assetbundleName 重复:{path}";
                LogUtils.Error(content);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                return false;
            }
            s_buildAssetLowerPaths.Add(lower);
            return true;
        }

        /// 对IAssetBundlePreCollector捕获的依赖物体， 单独进行打包
        private static void _ProcessIAssetBundlePreCollector(ref HashSet<string> processedFiles, List<string> blackList)
        {
            int errorCount = 0;
            var tempList = new List<string>();
            foreach (var assetBundleNode in s_buildAssetBundleNodeDict.Values)
            {
                tempList.AddRange(assetBundleNode.assetPaths);
            }
            foreach (var relativePath in tempList)
            {
                if(relativePath.EndsWith(".prefab"))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
                    if (obj != null)
                    {
                        var comps = obj.GetComponentsInChildren<IAssetBundlePreCollector>(true);
                        if (comps != null)
                        {
                            errorCount = 0;
                            foreach (var iter in comps)
                            {
                                CollectAssetBundlePaths(ref processedFiles, blackList, iter.CollectAssetBundlePath(), ref errorCount);
                            }

                            if (errorCount != 0)
                            {
                                LogUtils.Error($"IAssetBundlePreCollector配置错误: {relativePath}: errorCount: {errorCount}");
                            }
                        }
                    }
                }
                else if (relativePath.EndsWith(".asset"))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(relativePath);
                    if (asset is IAssetBundlePreCollector collector)
                    {
                        CollectAssetBundlePaths(ref processedFiles, blackList,
                            collector.CollectAssetBundlePath(), ref errorCount);
                    }
                }
            }
        }

        private static void CollectAssetBundlePaths(ref HashSet<string> processedFiles, List<string> blackList, IEnumerable<string> paths, ref int errorCount)
        {
            foreach (var subIter in paths)
            {
                string path = subIter.Replace(@"\", "/");
                if (FileUtils.FileExistsWithCaseSensitivity($"{AssetBundleData.s_projectPath}/{path}"))
                {
                    _AddRelativePath(ref processedFiles, blackList, path, 
                        "IAssetBundlePreCollector", "lutaian");
                }
                else
                {
                    ++errorCount;
                }
            }
        }

        private static bool _CheckPathLegal(HashSet<string> processedFiles, List<string> blackList,
            string relativePath)
        {
            if (processedFiles.Contains(relativePath))
            {
                return false;
            }

            if (blackList.Contains(relativePath))
            {
                return false;
            }

            if (!AssetBundleData.CanPackRes(relativePath))
            {
                return false;
            }

            return true;
        }

        private static void _AddRelativePath(ref HashSet<string> processedFiles, List<string> blackList,
            string relativePath, string funcName, string author)
        {
            if (!_CheckPathLegal(processedFiles, blackList, relativePath))
            {
                return;
            }


            processedFiles.Add(relativePath);
            Debug.Log(ZString.Format("{0}{1}","assetName : ", relativePath));

            if (!_CheckAssetNameDuplicate(relativePath))
            {
                return;
            }

            _UnifiedAddToPath(relativePath, author, funcName);
        }
        
         private static void _ProcessFilesAB(ref HashSet<string> processedFiles, List<Res_Filter_White> datas, List<string> blackList)
        {
            foreach (var iter in datas)
            {
                if (iter.filter_flag == 0)
                {
                    continue;
                }

                if (iter.one_ab == 0)
                {
                    string fullPath = ZString.Concat(AssetBundleData.s_projectPath, iter.path);
                    if (SysUtils.DirectoryExists(fullPath))
                    {
                        string[] files = null;
                        if (string.IsNullOrEmpty(iter.suffix))
                        {
                            files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);
                        }
                        else
                        {
                            files = Directory.GetFiles(fullPath, ZString.Format("{0}{1}", "*.", iter.suffix),
                                SearchOption.AllDirectories);
                        }
                        for (int i = 0; i < files.Length; i++)
                        {
                            string relativePath = AssetBundleData.AbsolutePathToUnityPath(files[i]);
                            _AddRelativePath(ref processedFiles, blackList, relativePath, 
                                "_ProcessFilesAB", "lutaian");
                        }
                    }
                }
            }
        }

        private static async void _ProcessCutSceneAB()
        {
            //var jsonPath = $"{AssetBundleData.s_appStreamingAssetsPath}/sundry/cutscene_preload.json";
            //var data = await FileUtils.DeserializeLoadJson<Dictionary<string, CutSceneDependencyInfo>>(jsonPath);
            //foreach (var cutSceneAssetPaths in data.Values)
            //{
            //    foreach (var cutSceneAssetPath in cutSceneAssetPaths.prefabList)
            //    {
            //        if (!s_buildAssetBundleNodeDict.ContainsKey(cutSceneAssetPath))
            //        {
            //            _CheckAssetNameDuplicate(cutSceneAssetPath);
            //            _UnifiedAddToPath(cutSceneAssetPath, "lutaian", "_ProcessFilesAB");
            //        }
            //    }
            //}
        }

        private static void _UnifiedAddToPath(string relativePath, string author, string funcName)
        {
            var fullPath = Application.dataPath.Replace("Assets", "") + relativePath;
            if (false == FileUtils.FileExistsWithCaseSensitivity(fullPath))
            {
                var content = $"ab路径配置错误，请联系{author},{funcName}, relativePath: {relativePath}";
                LogUtils.Error(content);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
            else
            {
                var assetBundleNode = CreateAssetBundleNode(relativePath);
                if (assetBundleNode != null)
                {
                    assetBundleNode.isRoot = true;
                }
            }
        }

        private static void _ProcessArrangeAB()
        {
            CombineFolderNodes(s_buildAssetBundleNodeDict);
            CheckLoopNode(s_buildAssetBundleNodeDict);
            for (int i = 0; i < 2; i++)
            {
                DeleteRedundantNode(s_buildAssetBundleNodeDict);
                CombineAssetBundleNodes(s_buildAssetBundleNodeDict);
                CombineParentEqualNodes(s_buildAssetBundleNodeDict);
            }
        }

        /// <summary>
        /// 递归创建ab资源及其引用
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static AssetBundleNode CreateAssetBundleNode(string path)
        {
            if (!s_buildAssetBundleNodeDict.TryGetValue(path, out var assetBundleNode))
            {
                if (AssetBundleData.CanPackRes(path))
                {
                    assetBundleNode = new AssetBundleNode();
                    assetBundleNode.path = path;
                    assetBundleNode.assetPaths.Add(path);
                    s_buildAssetBundleNodeDict.Add(path, assetBundleNode);
                    var assetDependencies = AssetDatabase.GetDependencies(path);
                    foreach (var iter in assetDependencies)
                    {
                        if (!AssetBundleData.CanPackRes(iter))
                        {
                            continue;
                        }

                        //大小写可能写错
                        if (iter.ToLower() == path.ToLower())
                        {
                            continue;
                        }

                        if (!s_buildAssetBundleNodeDict.TryGetValue(iter, out var dependAssetBundleNode))
                        {
                            dependAssetBundleNode = CreateAssetBundleNode(iter);
                        }

                        if (dependAssetBundleNode != null)
                        {
                            assetBundleNode.childNodeDict.Add(iter, dependAssetBundleNode);
                            dependAssetBundleNode.parentNodeDict.Add(path, assetBundleNode);
                        }
                    }
                }
            }
            return assetBundleNode;
        }

        /// <summary>
        /// 删除不需要的节点 A->B B->C A->C 删除A->C
        /// </summary>
        /// <param name="assetBundleNodeDict"></param>
        private static void DeleteRedundantNode(Dictionary<string, AssetBundleNode> assetBundleNodeDict)
        {
            bool isDel = true;
            while (isDel)
            {
                isDel = false;
                foreach (var assetBundleNode in assetBundleNodeDict.Values)
                {
                    var parentNodes = assetBundleNode.parentNodeDict.Values.ToArray();
                    foreach (var parentNode in parentNodes)
                    {
                        var childNodes = assetBundleNode.childNodeDict.Values.ToArray();
                        foreach (var childNode in childNodes)
                        {
                            isDel |= DeleteRedundantNode(parentNode, childNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 递归检查A->C，并删除相互依赖关系
        /// </summary>
        private static bool DeleteRedundantNode(AssetBundleNode parentNode, AssetBundleNode childNode)
        {
            bool isDel = false;
            if (parentNode.childNodeDict.ContainsKey(childNode.path))
            {
                parentNode.childNodeDict.Remove(childNode.path);
                childNode.parentNodeDict.Remove(parentNode.path);
                isDel = true;
            }
            var parentParentNodes = parentNode.parentNodeDict.Values.ToArray();
            foreach (var parentParentNode in parentParentNodes)
            {
                isDel |= DeleteRedundantNode(parentParentNode, childNode);
            }

            return isDel;
        }
        
        private static void CombineFolderNodes(Dictionary<string, AssetBundleNode> assetBundleNodeDict)
        {
            var assetBundleNodes = assetBundleNodeDict.Values.ToArray();
            foreach (var assetBundleNode in assetBundleNodes)
            {
                if (assetBundleNode.isFolder)
                {
                    var childNodes = assetBundleNode.childNodeDict.Values.ToArray();
                    foreach (var childNode in childNodes)
                    {
                        //if (childNode.parentNodeDict.Count != 2)
                        {
                            childNode.CombineNode(assetBundleNode);
                        }
                        /*else
                        {
                            //当文件夹节点的子节点有且只有一个其他父节点时
                            foreach (var childParentNode in childNode.parentNodeDict.Values)
                            {
                                if (childParentNode.path != assetBundleNode.path)
                                {
                                    childParentNode.assetPaths.AddRange(childNode.assetPaths);
                                    childParentNode.AddChildNodes(childNode.childNodeDict);
                                    break;
                                }
                            }
                        }*/
                        assetBundleNodeDict.Remove(childNode.path);
                    }
                }
            }
        }

        /// <summary>
        /// 对只有一个父节点的进行合并
        /// </summary>
        /// <param name="assetBundleNodeDict"></param>
        private static void CombineAssetBundleNodes(Dictionary<string, AssetBundleNode> assetBundleNodeDict)
        {
            bool combineFinish = false;
            while (!combineFinish)
            {
                combineFinish = true;
                var assetBundleNodes = assetBundleNodeDict.Values.ToArray();
                foreach (var assetBundleNode in assetBundleNodes)
                {
                    if (assetBundleNode.isRoot || assetBundleNode.parentNodeDict.Count != 1)
                    {
                        continue;
                    }
                    
                    var parentNode = assetBundleNode.parentNodeDict.Values.First();
                    assetBundleNode.CombineNode(parentNode);
                    assetBundleNodeDict.Remove(assetBundleNode.path);
                    combineFinish = false;
                }
            }
        }

        /// 递归设置node的isDelete为false
        private static void UnSetAssetBundleChildDelete(AssetBundleNode curNode)
        {
            foreach (var iter in curNode.childNodeDict)
            {
                iter.Value.isDelete = false;
                UnSetAssetBundleChildDelete(iter.Value);
            }
        }
        
        ///只将选中资源相关的ab，以及下游的ab都重新打包
        private static void FilterBySelect(Dictionary<string, AssetBundleNode> assetBundleNodeDict, HashSet<string> needSet)
        {
            //先全部标记为预删除
            foreach (var iter in assetBundleNodeDict)
            {
                iter.Value.isDelete = true;
            }
            
            foreach (var iter in assetBundleNodeDict)
            {
                var assetBundleNode = iter.Value;
                foreach (var subIter in assetBundleNode.assetPaths)
                {
                    if (needSet.Contains(subIter))
                    {
                        assetBundleNode.isDelete = false;
                        UnSetAssetBundleChildDelete(assetBundleNode);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 合并有相同父节点的到一个节点内
        /// </summary>
        /// <param name="assetBundleNodeDict"></param>
        private static void CombineParentEqualNodes(Dictionary<string, AssetBundleNode> assetBundleNodeDict)
        {
            var assetBundleNodes = assetBundleNodeDict.Values.ToArray();
            for (int i = 0; i < assetBundleNodes.Length; i++)
            {
                var assetBundleNodeA = assetBundleNodes[i];
                if (assetBundleNodeA.isDelete)
                {
                    continue;
                }

                for (int j = i + 1; j < assetBundleNodes.Length; j++)
                {
                    var assetBundleNodeB = assetBundleNodes[j];
                    if (assetBundleNodeB.isRoot || assetBundleNodeB.isDelete)
                    {
                        continue;
                    }

                    if (assetBundleNodeA.IsParentNodeEqual(assetBundleNodeB))
                    {
                        assetBundleNodeB.CombineNode(assetBundleNodeA);
                        assetBundleNodeDict.Remove(assetBundleNodeB.path);
                        assetBundleNodeB.isDelete = true;
                    }
                }
            }
        }

        private static void CheckLoopNode(Dictionary<string, AssetBundleNode> assetBundleNodeDict)
        {
            foreach (var assetBundleNode in assetBundleNodeDict.Values)
            {
                if (assetBundleNode.CheckLoopNode(assetBundleNode))
                {
                    var content = "hyper-- Error, ab存在嵌套依赖 : " + assetBundleNode.path;
                    LogUtils.Error(content);
                    if (Application.isBatchMode)
                    {
                        EditorApplication.Exit(1);
                    }
                    throw new Exception("循环引用中止");
                }
            }
        }

        private static void _GetAllABInfo()
        {
            s_buildAssetLowerPaths.Clear();
            s_buildAssetBundleNodeDict.Clear();

            List<string> blackList = null;
            var datas = GetConfigDatas(ref blackList);

            if (datas == null)
            {
                return;
            }

            HashSet<string> processedFiles = new HashSet<string>();

            //先处理单个文件的ab， 再处理单文件夹的ab， 最后处理普通ab
            _ProcessSpecifiedFileAB(ref processedFiles, datas, blackList);
            _ProcessSubFolderAB(ref processedFiles, datas, blackList);
            _ProcessFolderAB(ref processedFiles, datas, blackList);
            _ProcessFilesAB(ref processedFiles, datas, blackList);
            _ProcessIAssetBundlePreCollector(ref processedFiles, blackList);
            //_ProcessCutSceneAB();
            
            _ProcessArrangeAB();
        }

        public static void BuildAllAssetBundle(BuildTarget target, bool regenerate = false)
        {
            LogUtils.Debug("hyper-- start to BuildAllAssetBundle");
            if (HasDirtyScenes())
            {
                LogUtils.Error("有未保存的场景，请保存后再Build AssetBundle");
                return;
            }
            s_targetPlatform = target;
            //JenkinsWwise.CopyWwiseAudio();
            
            _GetAllABInfo();

            _BuildAssetBundleBuilds(s_targetPlatform, AssetBundleData.ASSETBUNDLE_ROOT_PATH, regenerate);
            LogUtils.Debug("hyper-- end to BuildAllAssetBundle");
        }
        
        private static bool HasDirtyScenes()
        {
            var unsavedChanges = false;
            var sceneCount = EditorSceneManager.sceneCount;
            for (var i = 0; i < sceneCount; ++i)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (!scene.isDirty)
                    continue;
                unsavedChanges = true;
                break;
            }

            return unsavedChanges;
        }

        /// <summary>
        /// 获取黑名单，这些列举的路径不会被打包进ab
        /// </summary>
        private static List<string> _FilterBlackFiles(ExcelTable table)
        {
            List<string> blackList = new List<string>();
            int act_col = 0;
            for (int i = 1; i <= table.NumberOfColumns; i++)
            {
                var title = table.GetValue(1, i).ToString();
                if (title.Equals("path"))
                {
                    act_col = i;
                    break;
                }
            }

            for (int i = 4; i <= table.NumberOfRows; i++)
            {
                string act_id = table.GetValue(i, act_col).ToString();
                if (act_id.Equals(""))
                    continue;

                blackList.Add(act_id.Replace("\\", "/"));
            }

            return blackList;
        }

        /// <summary>
        /// 删除manifest文件
        /// </summary>
        private static void _DeleteAbandonedFiles(string folderPath)
        {
            var allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            foreach (var iter in allFiles)
            {
                var fullPath = iter.Replace("\\", "/");

                if (fullPath.EndsWith(".manifest"))
                {
                    SysUtils.DeleteFile(fullPath);
                    continue;
                }
                
                if (fullPath.Contains("buildlogtep.json"))
                {
                    SysUtils.DeleteFile(fullPath);
                    continue;
                }

                if (fullPath.EndsWith(".manifest.meta"))
                {
                    SysUtils.DeleteFile(fullPath);
                }
            }
        }
        
        public static void ClearAssetBundleAndCache(string folderPath)
        {
            UnityEditor.Build.Pipeline.Utilities.BuildCache.PurgeCache(false);
            if (SysUtils.DirectoryExists(folderPath))
            {
                SysUtils.DeleteDirectory(folderPath);
            }
            AssetDatabase.Refresh();
        }

        //private static void SetSbpNoMeshFbxPath()
        //{
        //    var data = AssetDatabase.LoadAssetAtPath<ModelCombineData>(ModelCombine.s_dataPath);
        //    if (data != null && data.isProjectCombine)
        //    {
        //        CalculateAssetDependencyData.noMeshFbxPaths = data.fbxPaths;
        //    }
        //}

        private static void _BuildAssetBundleBuilds(BuildTarget targetPlatform, string key, bool regenerate)
        {
            //SetSbpNoMeshFbxPath();
            var relativePath = $"Assets/StreamingAssets/{key}";
            //folderPath:  D:\MainLine_Full\Assets\StreamingAssets\ab
            var folderPath = $"{AssetBundleData.s_appStreamingAssetsPath}/{key}";
            
            if (regenerate)
            {
                ClearAssetBundleAndCache(folderPath);
            }
            SysUtils.MakeSureDirExist(folderPath);
            List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
            foreach (var assetBundleNode in s_buildAssetBundleNodeDict.Values)
            {
                //兼容temp下的打包逻辑
                if (assetBundleNode.isDelete)
                {
                    continue;
                }
                AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
                assetBundleBuild.assetBundleName = AssetBundleData.GetPackAbName(assetBundleNode.path);
                assetBundleBuild.assetNames = assetBundleNode.assetPaths.ToArray();
                assetBundleBuilds.Add(assetBundleBuild);
            }
            var options = BuildAssetBundleOptions.ChunkBasedCompression;

            if (regenerate)
            {
                options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }
            CompatibilityBuildPipeline.BuildAssetBundles(relativePath, assetBundleBuilds.ToArray(),
                options , targetPlatform);

            SaveAssetBundleInfo(s_buildAssetBundleNodeDict, AssetBundleData.s_abInfoPath);
            _DeleteAbandonedFiles(folderPath);
            
            AssetDatabase.Refresh();
        }

        private static void _CopySpecificAbToProjectAndroid(string tempPath, string buildKeyPath)
        {
            string projectPath = Application.dataPath.Replace("/Assets", $"/Build/{buildKeyPath}/");
            projectPath = $"{projectPath}elrond/unityLibrary/src/main/assets/{AssetBundleData.ASSETBUNDLE_ROOT_PATH}";
            if (SysUtils.DirectoryExists(projectPath))
            {
                SysUtils.CopyDirectory(tempPath, projectPath);
            }
        }
        
        private static void _CopySpecificAbToProjectiOS(string tempPath, string buildKeyPath)
        {
            string projectPath = Application.dataPath.Replace("/Assets", $"/Build/{buildKeyPath}/");
            projectPath = $"{projectPath}elrond/Data/Raw/{AssetBundleData.ASSETBUNDLE_ROOT_PATH}";

            if (SysUtils.DirectoryExists(projectPath))
            {
                SysUtils.CopyDirectory(tempPath, projectPath);
            }
        }

        /// <summary>
        /// 尝试将ab和依赖关系拷贝到之前导出的项目中
        /// </summary>
        private static void _CopySpecificAbToProject()
        {
            string tempPath = $"{AssetBundleData.s_appStreamingAssetsPath}/{AssetBundleData.ASSETBUNDLE_ROOT_TEMP}";
            var allLanguage = new List<string> {"cn", "jp", "kr", "en"};
    #if UNITY_ANDROID
            foreach (var iter in allLanguage)
            {
                var buildKeyPath = $"an_{iter}_d";
                _CopySpecificAbToProjectAndroid(tempPath, buildKeyPath);
                
                buildKeyPath = $"an_{iter}_r";
                _CopySpecificAbToProjectAndroid(tempPath, buildKeyPath);
            }
    #elif UNITY_IPHONE
            foreach (var iter in allLanguage)
            {
                var buildKeyPath = $"ios_{iter}_d";
                _CopySpecificAbToProjectiOS(tempPath, buildKeyPath);
                
                buildKeyPath = $"ios_{iter}_r";
                _CopySpecificAbToProjectiOS(tempPath, buildKeyPath);
            }
    #endif
        }

        /// <summary>
        /// 尝试将ab和依赖关系拷贝到StreamingAssets中
        /// </summary>
        private static void _CopySpecificAbToStreamingAssets()
        {
            string abPath = AssetBundleData.s_abFullFolderPath;
            string tempPath = $"{AssetBundleData.s_appStreamingAssetsPath}/{AssetBundleData.ASSETBUNDLE_ROOT_TEMP}";

            SysUtils.CopyDirectory(tempPath, abPath, SysUtils.FilterFile);
        }

#if UNITY_EDITOR
        private static int stringCacheID = 0;
        private static int GetStringCacheID(string path, ref List<string> stringCacheList)
        {
            var index = stringCacheList.IndexOf(path);
            
            if (index == -1)
            {
                stringCacheList.Add(path);
                index = stringCacheID;
                ++stringCacheID;
            }
            return index;
        }

        public static async UniTask CreateOldJsonFromNewJson()
        {
            var allInfo = await FileUtils.DeserializeLoadJson<AssetBundleInfo>(AssetBundleData.s_abInfoPath);
            var backupPath = $"{AssetBundleData.s_abFullFolderPath}/ab_方便检索版.json";
            
            var oldInfo = new AssetBundleInfo_Old();
            var subList = new List<string>();
            foreach (var iter in allInfo.abDependenciesDict)
            {
                foreach (var subIter in iter.Value)
                {
                    subList.Add(allInfo.assetPathArray[subIter]);
                }
                oldInfo.abDependenciesDict[iter.Key] = subList.ToArray();
                subList.Clear();
            }

            foreach (var iter in allInfo.assetPathInAbPathDict)
            {
                oldInfo.assetPathInAbPathDict[iter.Key] = allInfo.assetPathArray[iter.Value];
            }
            FileUtils.SerializeSaveJson(backupPath, oldInfo);
        }
#endif

        private static void SaveAssetBundleInfo(Dictionary<string, AssetBundleNode> assetBundleNodes, string jsonPath)
        {
            var assetPathInAbPathDict = new Dictionary<string, int>();
            var abDependenciesDict = new Dictionary<string, int[]>();
            var stringCacheList = new List<string>();
            stringCacheID = 0;

            var oldInfo = new AssetBundleInfo_Old();
            
            foreach (var assetBundleNode in assetBundleNodes.Values)
            {
                string abName = AssetBundleData.GetPackAbName(assetBundleNode.path);
                var stringCacheID = GetStringCacheID(abName, ref stringCacheList);
                
                if (assetBundleNode.isFolder)
                {
                    assetPathInAbPathDict.Add(assetBundleNode.path, stringCacheID);
                    oldInfo.assetPathInAbPathDict.Add(assetBundleNode.path, abName);
                }
                foreach (var assetPath in assetBundleNode.assetPaths)
                {
                    assetPathInAbPathDict.Add(assetPath, stringCacheID);
                    oldInfo.assetPathInAbPathDict.Add(assetPath, abName);
                }

                if (assetBundleNode.childNodeDict.Count > 0)
                {
                    var enumerable =
                        assetBundleNode.childNodeDict.Keys.Select(t => AssetBundleData.GetPackAbName(t));
                    string[] abDependencies = enumerable.ToArray();
                    List<int> stringCacheIDList = new List<int>();
                    foreach (var iter in abDependencies)
                    {
                        stringCacheIDList.Add(GetStringCacheID(iter, ref stringCacheList));
                    }
                    abDependenciesDict.Add(abName, stringCacheIDList.ToArray());
                    oldInfo.abDependenciesDict.Add(abName, abDependencies);
                }
            }
            AssetBundleInfo assetBundleInfo = new AssetBundleInfo(stringCacheList, assetPathInAbPathDict, abDependenciesDict);
            
            stringCacheID = 0;
            FileUtils.SerializeSaveJson(jsonPath, assetBundleInfo);

            var assistantPath = $"{AssetBundleData.s_projectPath}/Build/assistants";
            SysUtils.MakeSureDirExist(assistantPath);
            
            var backupPath = $"{assistantPath}/ab.json";
            FileUtils.SerializeSaveJson(backupPath, oldInfo);
        }

        [MenuItem("Assets/HGTools/选中Assets Build AssetBundle", priority = 1)]
        public static void BuildSelectAssets()
        {
            var selected = Selection.objects;
            if (selected == null)
            {
                return;
            }

            var neededSet = _GetAllSelectedPath(selected);
            s_buildAssetLowerPaths.Clear();
            s_buildAssetBundleNodeDict.Clear();

            foreach (var path in neededSet)
            {
                var assetBundleNode = CreateAssetBundleNode(path);
                if (assetBundleNode != null)
                {
                    assetBundleNode.isRoot = true;
                }
            }
            List<string> blackList = null;
            var datas = GetConfigDatas(ref blackList);
            if (datas == null)
            {
                return;
            }
            HashSet<string> processedFiles = new HashSet<string>();
            _ProcessIAssetBundlePreCollector(ref processedFiles, blackList);
            _ProcessArrangeAB();
    #if UNITY_ANDROID
            s_targetPlatform = BuildTarget.Android;
    #elif UNITY_IPHONE
            s_targetPlatform = BuildTarget.iOS;
    #else
            s_targetPlatform = BuildTarget.StandaloneWindows64;
    #endif
            _BuildAssetBundleBuilds(s_targetPlatform, AssetBundleData.ASSETBUNDLE_ROOT_PATH, true);
        }


        [MenuItem("Assets/HGTools/刷新AssetBunble并拷贝到工程", priority = 1)]
        public static void BuildSpecificABAndCopy()
        {
            var selected = Selection.objects;
            if (selected == null)
            {
                return;
            }

            var neededSet = _GetAllSelectedPath(selected);

            foreach (var iter in neededSet)
            {
                if (iter.EndsWith(".cs"))
                {
                    EditorUtility.DisplayDialog("warning", "选中文件涉及到了cs文件，请选择完整版打包", "我知道了");
                    return;
                }
            }

            _GetAllABInfo();
            FilterBySelect(s_buildAssetBundleNodeDict, neededSet);

    #if UNITY_ANDROID
            s_targetPlatform = BuildTarget.Android;
    #elif UNITY_IPHONE
            s_targetPlatform = BuildTarget.iOS;
    #else
            s_targetPlatform = BuildTarget.StandaloneWindows64;
    #endif
            
            var folderPath = $"{AssetBundleData.s_appStreamingAssetsPath}/{AssetBundleData.ASSETBUNDLE_ROOT_TEMP}";
            if (SysUtils.DirectoryExists(folderPath))
            {
                //编辑器下保证temp文件夹是新建的
                SysUtils.DeleteDirectory(folderPath);
            }

            _BuildAssetBundleBuilds(s_targetPlatform, AssetBundleData.ASSETBUNDLE_ROOT_TEMP, false);

            _CopySpecificAbToStreamingAssets();
            _CopySpecificAbToProject();
            
            if (SysUtils.DirectoryExists(folderPath))
            {
                SysUtils.DeleteDirectory(folderPath);
            }

            EditorUtility.DisplayDialog("拷贝结束", "请继续操作AndroidStudio或Xcode", "我知道了");
        }

        /// <summary>
        /// 获取选中的组件的路径，文件夹会被映射成所有子文件
        /// </summary>
        private static HashSet<string> _GetAllSelectedPath(UnityEngine.Object[] objects)
        {
            HashSet<string> neededSet = new HashSet<string>();

            foreach (var iter in objects)
            {
                var path = AssetDatabase.GetAssetPath(iter).Replace("\\", "/");
                var fullPath = Application.dataPath + "/" + path.Replace("Assets/", "");
                if (SysUtils.IsDir(fullPath))
                {
                    var files = SysUtils.GetAllFiles(fullPath, "*.*",
                        SearchOption.AllDirectories).Where(s => false == s.EndsWith(".meta"));
                    foreach (var subIter in files)
                    {
                        var subRelativePath = subIter.Substring(subIter.IndexOf("Assets/")).Replace("\\", "/");
                        neededSet.Add(subRelativePath);
                    }
                }
                else
                {
                    neededSet.Add(path);
                }
            }

            return neededSet;
        }

    }
}

