using System.Collections.Generic;

namespace AssetLoad
{

    public class AssetBundleInfo_Old
    {
        //key: asset relative path,  value: abname(could be same)
        public readonly Dictionary<string, string> assetPathInAbPathDict = new Dictionary<string, string>();

        //key: parent ab   value: child ab
        public readonly Dictionary<string, string[]> abDependenciesDict = new Dictionary<string, string[]>();
    }

    public class AssetBundleInfo
    {
        public readonly string[] assetPathArray = new string[]{};
        //key: asset relative path,  value: abname(could be same)
        public readonly Dictionary<string, int> assetPathInAbPathDict = new Dictionary<string, int>();
        //key: parent ab   value: child ab
        public readonly Dictionary<string, int[]> abDependenciesDict = new Dictionary<string, int[]>();

        public AssetBundleInfo(List<string> pathCacheList, Dictionary<string, int> curAssetPathInAbPathDict, Dictionary<string, int[]> curAbDependenciesDict)
        {
            assetPathInAbPathDict = curAssetPathInAbPathDict;
            abDependenciesDict = curAbDependenciesDict;
            assetPathArray = pathCacheList.ToArray();
        }

        public string GetRelativePathByID(int ID)
        {
            if (assetPathArray.Length > ID)
            {
                return assetPathArray[ID];
            }

            return null;
        }
    }
}