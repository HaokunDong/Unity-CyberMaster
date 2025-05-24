using Cysharp.Text;
using Everlasting.Extend;
using UnityEngine;

namespace Tools
{
    public class LoadUtils
    {
        private static string s_appStreamingAssets = ZString.Concat(Application.streamingAssetsPath, "/");
        
        /// <summary>
        /// 获取正确的ab所在路径
        /// </summary>
        /// <param name="resPath">项目内资源路径</param>
        /// <returns></returns>
        public static string GetAbLoadPath(string resPath)
        {
            string abPath = ZString.Concat(AssetBundleData.ASSETBUNDLE_ROOT_PATH, "/", resPath);
            abPath = GetStreamingAssetsLoadPath(abPath);
            return abPath;
        }

        /// <summary>
        /// 获取正确的加载路径
        /// </summary>
        /// <param name="streamingAssetsPath">StreamingAssets下的资源路径</param>
        /// <returns></returns>
        public static string GetStreamingAssetsLoadPath(string streamingAssetsPath)
        {
            return ZString.Concat(
                //IsHotUpdateDownloaded(streamingAssetsPath)
                    //? HGDownloadData.s_downloadedRootPath
                    //: 
                Application.streamingAssetsPath, "/", streamingAssetsPath);
        }

        public static string GetLoadPath(string fullPath)
        {
            string loadPath = fullPath;
            string streamingAssetsPath = fullPath.SubTailLastOf(s_appStreamingAssets);
            if (IsHotUpdateDownloaded(streamingAssetsPath))
            {
                //loadPath = ZString.Concat(HGDownloadData.s_downloadedRootPath, "/", streamingAssetsPath);
            }
            return loadPath;
        }

        private static bool IsHotUpdateDownloaded(string path)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return false;
            }
#endif
            //return HotUpdateDownloadCtrl.Instance.IsDownloaded(path);
            return false;
        }
    }
}