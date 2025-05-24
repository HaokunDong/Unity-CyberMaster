using System.Collections.Generic;

namespace AssetLoad
{
    //预收集需要打包的资源路径
    public interface IAssetBundlePreCollector
    {
#if UNITY_EDITOR
        IEnumerable<string> CollectAssetBundlePath();
#endif
    }
}