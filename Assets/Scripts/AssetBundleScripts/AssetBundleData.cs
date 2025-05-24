using Cysharp.Text;
using Tools;
using UnityEngine;

public class AssetBundleData
{
    public static bool s_assetBundleLoad = false;

    public const string EXT_NAME = ".ab";
    public const string ASSETBUNDLE_ROOT_PATH = "ab";
    public const string ASSETBUNDLE_ROOT_TEMP = "temp";  //不能和 ASSETBUNDLE_ROOT_PATH 相同
    public static readonly string[] DEP_RES_EXCEPT_EXT_NAMES = new string[] { ".cs", ".psd", ".meta", ".DS_Store" };
    public static readonly string[] DEP_RES_EXCEPT_TAG_NAMES = new string[] { "DoNotPack" };

    ///     D:/MainLine_Full/
    public static string s_projectPath => Application.dataPath.Replace("Assets", "");

    public static string s_appStreamingAssetsPath => Application.streamingAssetsPath;

    /// <summary>
    /// ab文件夹完整路径(仅Editor用)
    /// </summary>
    public static string s_abFullFolderPath => ZString.Concat(s_appStreamingAssetsPath, "/", ASSETBUNDLE_ROOT_PATH);

    public static string s_abInfoPath => LoadUtils.GetAbLoadPath("ab.json");

    /// <summary>
    /// 绝对路径转Unity路径
    /// </summary>
    /// <param name="absolutePath"></param>
    /// <returns></returns>
    public static string AbsolutePathToUnityPath(string absolutePath)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.Append(absolutePath);
            sb.Replace(s_projectPath,"");
            sb.Replace('\\','/');
            return sb.ToString();
        }
    }

    public static string GetPackAbName(string path)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.Append(path);
            sb.Append(EXT_NAME);
            return sb.ToString().ToLower();
        }
    }

    public static bool CanPackRes(string path)
    {
        bool canPack = true;
        for (int i = 0; i < DEP_RES_EXCEPT_EXT_NAMES.Length; i++)
        {
            if (path.EndsWith(DEP_RES_EXCEPT_EXT_NAMES[i]) || path.ToLower().EndsWith(DEP_RES_EXCEPT_EXT_NAMES[i]))
            {
                canPack = false;
                break;
            }
        }
        for (int i = 0; i < DEP_RES_EXCEPT_TAG_NAMES.Length; i++)
        {
            if (path.Contains(DEP_RES_EXCEPT_TAG_NAMES[i]) || path.ToLower().Contains(DEP_RES_EXCEPT_TAG_NAMES[i]))
            {
                canPack = false;
                break;
            }
        }
        return canPack;
    }
}
