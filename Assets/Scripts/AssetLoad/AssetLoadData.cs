using Cysharp.Threading.Tasks;
using GameBase.Log;
using UnityEngine;

namespace AssetLoad
{
    public abstract class AssetLoadDataBase<T> where T : class
    {
        protected readonly string path;
        protected T asset;
        protected AsyncLazy<T> loadingTask;
        public int RefCount{ get; protected set; }

        protected AssetLoadDataBase(string path)
        {
            this.path = path;
        }

        public virtual T GetAsset()
        {
            RefCount++;
            return asset;
        }

        public void AddRef()
        {
            RefCount++;
        }

        public void Unload()
        {
            RefCount--;
            if (RefCount <= 0)
            {
                Dispose();
            }
        }

        public virtual void Dispose()
        {
            asset = null;
            RefCount = 0;
            LogUtils.Trace($"卸载资源: {path}", LogChannel.Load);
        }
    }

    public class BundleLoadData : AssetLoadDataBase<AssetBundle>
    {
        private AssetBundleCreateRequest m_assetBundleCreateRequest;
        
        public BundleLoadData(string path) : base(path)
        {
        }

        public override AssetBundle GetAsset()
        {
            if (asset == null)
            {
                //异步转同步时不添加引用计数
                if (m_assetBundleCreateRequest != null)
                {
                    LogUtils.Warning($"同步加载异步未完成ab: {path}");
                    asset = m_assetBundleCreateRequest.assetBundle;
                    m_assetBundleCreateRequest = null;
                }
                else
                {
                    asset = LoadBundle();
                }
                return asset;
            }
            return base.GetAsset();
        }


        public AssetBundle LoadBundle()
        {
            RefCount++;
            if (asset == null)
            {
                asset = AssetBundle.LoadFromFile(path);
                if (asset == null)
                {
                    LogUtils.Error($"同步加载ab失败: {path}");
                } 
                else
                {
                    LogUtils.Trace($"同步加载ab成功: {path}", LogChannel.Load);
                }
            }

            return asset;
        }

        public UniTask<AssetBundle> LoadBundleAsync()
        {
            RefCount++;
            if (asset == null)
            {
                loadingTask ??= UniTask.Lazy(async () =>
                {
                    m_assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(path);
                    asset = await m_assetBundleCreateRequest;
                    m_assetBundleCreateRequest = null;
                    if (asset == null)
                    {
                        LogUtils.Error($"异步加载ab失败: {path}");
                    }
                    else
                    {
                        LogUtils.Trace($"异步加载ab成功: {path}", LogChannel.Load);
                    }

                    loadingTask = null;
                    return asset;
                });
                return loadingTask.Task;
            }
            return UniTask.FromResult(asset);
        }

        public override void Dispose()
        {
            m_assetBundleCreateRequest = null;
            if (asset != null)
            {
                LogUtils.Trace($"AssetBundle Unload(true): {path}", LogChannel.Load);
                asset.Unload(true);
            }
            base.Dispose();
        }
    }

    public class ObjectLoadData : AssetLoadDataBase<Object>
    {
        private AssetBundle m_assetBundle;
        private UniTask<AssetBundle> m_assetBundleTask;
        public bool IsAssetBundleLoaded => m_assetBundle != null;
        
        public ObjectLoadData(string path) : base(path)
        {
            
        }

        public ObjectLoadData(UniTask<AssetBundle> assetBundleTask, string path) : base(path)
        {
            m_assetBundleTask = assetBundleTask;
        }

        public void SetAssetBundle(AssetBundle assetBundle)
        {
            m_assetBundle = assetBundle;
        }

        public override Object GetAsset()
        {
            if (asset == null)
            {
                LogUtils.Warning($"同步加载异步未完成Object:{path}");
                asset = LoadObject<Object>();
                return asset;
            }

            return base.GetAsset();
        }

        public Object LoadObject<T>()
        {
            RefCount++;
            if (asset == null)
            {
                if (m_assetBundle != null)
                {
                    asset = m_assetBundle.LoadAsset(path, typeof(T));
                    if (asset == null)
                    {
                        LogUtils.Error($"同步加载object失败: {path}");
                    }
                    else
                    {
                        LogUtils.Trace($"同步加载object成功: {path}", LogChannel.Load);
                    }
                }
                else
                {
                    LogUtils.Error($"同步加载object ab为空: {path}");
                }
            }
            return asset;
        }

        public UniTask<Object> LoadObjectAsync<T>()
        {
            RefCount++;
            if (asset == null)
            {
                loadingTask ??= UniTask.Lazy(async () =>
                {
                    if (m_assetBundle == null)
                    {
                        m_assetBundle = await m_assetBundleTask;
                    }
                    asset = await m_assetBundle.LoadAssetAsync(path, typeof(T));
                    if (asset == null)
                    {
                        LogUtils.Error($"异步加载object失败: {path}");
                    }
                    else
                    {
                        LogUtils.Trace($"异步加载object成功: {path}", LogChannel.Load);
                    }
                    loadingTask = null;
                    return asset;
                });
                return loadingTask.Task;
            }
            return UniTask.FromResult(asset);
        }
    }
}