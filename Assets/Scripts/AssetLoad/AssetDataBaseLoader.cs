#if UNITY_EDITOR
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameBase.Log;
using UnityEngine;
using UnityEditor;
using Cysharp.Text;

namespace AssetLoad
{
    public class AssetDataBaseLoader: AssetLoaderBase, IAssetLoader
    {
        /// <summary>
        /// 已加载asset
        /// </summary>
        private Dictionary<string, Object> m_loadedAssetDict = new Dictionary<string, Object>();
        
        public UniTask Init()
        {
            return UniTask.CompletedTask;
        }

        public async UniTask<Object> LoadAssetAsync<T>(string path, bool folder = false)
        {
            if (folder)
            {
                return null;
            }
            else
            {
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(T));

                if (asset == null) {
                    LogUtils.Error(ZString.Format ("加载资源失败: {0}", path));
                }
                m_loadedAssetDict[path] = asset;
                bool isPlayingLoad = Application.isPlaying;
                //添加延迟模拟异步加载
                int delayFrame = Random.Range(1, 6);
                await UniTask.DelayFrame(delayFrame);
                LogUtils.Trace(ZString.Format("加载资源成功: {0}", path), LogChannel.Load);
                if (!Application.isPlaying && isPlayingLoad)
                {
                    return null;
                }
                return asset;
            }
        }

        public Object LoadAsset<T>(string path)
        {
            if (m_loadedAssetDict.TryGetValue(path, out var asset))
            {
                return asset;
            }
            
            asset = AssetDatabase.LoadAssetAtPath(path, typeof(T));

            if (asset == null) {
                LogUtils.Error(ZString.Format ("加载资源失败: {0}", path));
            }
            m_loadedAssetDict[path] = asset;
            LogUtils.Trace(ZString.Format("加载资源成功: {0}", path), LogChannel.Load);
            return asset;

        }

        public bool IsLoaded(string path)
        {
            return m_loadedAssetDict.ContainsKey(path);
        }

        public void AddAssetRef(string path)
        {
            
        }

        public int UnLoadAsset(string path)
        {
            return 1;
        }
    }
}
#endif