using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameBase.Log;
using Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetLoad
{
    public class AssetBundleLoader : AssetLoaderBase, IAssetLoader
    {
        private AssetBundleInfo m_assetBundleInfo;
        
        /// <summary>
        /// 加载ab文件数据
        /// </summary>
        private Dictionary<string, BundleLoadData> m_bundleLoadDataDict = new Dictionary<string, BundleLoadData>(200);
        /// <summary>
        /// 加载object文件数据
        /// </summary>
        private Dictionary<string, ObjectLoadData> m_objectLoadDataDict = new Dictionary<string, ObjectLoadData>(200);
        

        public async UniTask Init()
        {
            m_assetBundleInfo ??= await FileUtils.DeserializeLoadJson<AssetBundleInfo>(AssetBundleData.s_abInfoPath);
        }

        public Object LoadAsset<T>(string path)
        {
            if (m_assetBundleInfo == null)
            {
                LogUtils.Error("AssetBundleInfo未加载");
                return null;
            }
            if (m_objectLoadDataDict.TryGetValue(path, out var objectLoadData))
            {
                if (objectLoadData.IsAssetBundleLoaded)
                {
                    return objectLoadData.GetAsset();
                }
            }
            else
            {
                objectLoadData = new ObjectLoadData(path);
                m_objectLoadDataDict.Add(path, objectLoadData);
            }
            //LogUtils.Debug("同步加载: " + path);
            var abName = GetAbNameByRelativePath(path);
            if (abName == null)
            {
                LogUtils.Error($"资源没有找到ab，请检查路径: {path}");
                return null;
            }
            AssetBundle assetBundle = _LoadBundleWithDepend(abName);
            objectLoadData.SetAssetBundle(assetBundle);
            return objectLoadData.LoadObject<T>();
        }

        private string GetAbNameByRelativePath(string path)
        {
            if (m_assetBundleInfo.assetPathInAbPathDict.TryGetValue(path, out int ID))
            {
                if (m_assetBundleInfo.assetPathArray.Length > ID)
                {
                    return m_assetBundleInfo.assetPathArray[ID];
                }
            }

            return null;
        }
        
        /// <summary>
        /// 同步加载ab及依赖ab
        /// </summary>
        private AssetBundle _LoadBundleWithDepend(string abName)
        {
            if (string.IsNullOrEmpty(abName))
            {
                return null;
            }
            
            if (m_bundleLoadDataDict.TryGetValue(abName, out var bundleLoadData))
            {
                return bundleLoadData.GetAsset();
            }
            
            string abPath = LoadUtils.GetAbLoadPath(abName);
            AssetBundle loadAssetBundle = _LoadBundle(abName, abPath);
            if (m_assetBundleInfo.abDependenciesDict.TryGetValue(abName, out var dependencies))
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    _LoadBundleWithDepend(m_assetBundleInfo.GetRelativePathByID(dependencies[i]));
                }
            }
            return loadAssetBundle;
        }
        
        /// <summary>
        /// 同步加载单个ab
        /// </summary>
        private AssetBundle _LoadBundle(string abName, string abPath)
        {
            if (m_bundleLoadDataDict.TryGetValue(abName, out var bundleLoadData))
            {
                return bundleLoadData.GetAsset();
            }
            bundleLoadData = new BundleLoadData(abPath);
            m_bundleLoadDataDict.Add(abName, bundleLoadData);
            return bundleLoadData.LoadBundle();
        }

        public async UniTask<Object> LoadAssetAsync<T>(string path, bool folder = false)
        {
            if (m_assetBundleInfo == null)
            {
                await Init();
            }
            if (m_objectLoadDataDict.TryGetValue(path, out ObjectLoadData objectLoadData))
            {
                return await objectLoadData.LoadObjectAsync<T>();
            }
            var abName = GetAbNameByRelativePath(path);
            if (abName == null)
            {
                LogUtils.Error($"资源没有找到ab，请检查路径: {path}");
                return null;
            }
            
            if (!folder)
            {
                var assetBundle = _LoadBundleAsyncWithDepend(abName);
                objectLoadData = new ObjectLoadData(assetBundle, path);
                m_objectLoadDataDict.Add(path, objectLoadData);
                return await objectLoadData.LoadObjectAsync<T>();
            }
            else
            {
                await _LoadBundleAsyncWithDepend(abName);
                return null;
            }
        }

        /// <summary>
        /// 异步加载ab及依赖ab
        /// </summary>
        private async UniTask<AssetBundle> _LoadBundleAsyncWithDepend(string abName)
        {
            if (string.IsNullOrEmpty(abName))
            {
                return null;
            }

            if (m_bundleLoadDataDict.TryGetValue(abName, out var bundleLoadData))
            {
                return await bundleLoadData.LoadBundleAsync();
            }

            string abPath = LoadUtils.GetAbLoadPath(abName);
            IEnumerable<UniTask<AssetBundle>> LoadDependencies()
            {
                yield return _LoadBundleAsync(abName, abPath);
                if (m_assetBundleInfo.abDependenciesDict.TryGetValue(abName, out var dependencies))
                {
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        yield return _LoadBundleAsyncWithDepend(m_assetBundleInfo.GetRelativePathByID(dependencies[i]));
                    }
                }
            }
            var loadedBundle =  await UniTask.WhenAll(LoadDependencies());
            return loadedBundle[0];
        }

        /// <summary>
        /// 异步加载单个ab
        /// </summary>
        private async UniTask<AssetBundle> _LoadBundleAsync(string abName, string abPath)
        {
            if (m_bundleLoadDataDict.TryGetValue(abName, out var bundleLoadData))
            {
                return await bundleLoadData.LoadBundleAsync();
            }
            bundleLoadData = new BundleLoadData(abPath);
            m_bundleLoadDataDict.Add(abName, bundleLoadData);
            return await bundleLoadData.LoadBundleAsync();
        }

        public bool IsLoaded(string path)
        {
            return m_objectLoadDataDict.ContainsKey(path);
        }

        public void AddAssetRef(string path)
        {
            if (m_objectLoadDataDict.TryGetValue(path, out var objectLoadData))
            {
                if (objectLoadData.IsAssetBundleLoaded)
                {
                    objectLoadData.AddRef();
                }
                else
                {
                    LogUtils.Error($"错误添加加载中资源引用计数: {path}");
                }
            }
            else
            {
                LogUtils.Error($"错误添加未加载资源引用计数: {path}");
            }
        }

        public int UnLoadAsset(string path)
        {
            int leftRefCount = 0;
            if (m_objectLoadDataDict.TryGetValue(path, out var objectLoadData))
            {
                objectLoadData.Unload();
                leftRefCount = objectLoadData.RefCount;
                if (objectLoadData.RefCount <= 0)
                {
                    var abName = GetAbNameByRelativePath(path);
                    if (abName != null)
                    {
                        UnLoadBundle(abName);
                    }

                    m_objectLoadDataDict.Remove(path);
                }
            }

            return leftRefCount;
        }

        private void UnLoadBundle(string abName)
        {
            if (string.IsNullOrEmpty(abName))
            {
                return;
            }
            
            if (m_bundleLoadDataDict.TryGetValue(abName, out var bundleLoadData))
            {
                bundleLoadData.Unload();
                if (bundleLoadData.RefCount <= 0)
                {
                    if (m_assetBundleInfo.abDependenciesDict.TryGetValue(abName, out var dependencies))
                    {
                        for (int i = 0; i < dependencies.Length; i++)
                        {
                            UnLoadBundle(m_assetBundleInfo.GetRelativePathByID(dependencies[i]));
                        }
                    }

                    m_bundleLoadDataDict.Remove(abName);
                }
            }
        }

    }
}

