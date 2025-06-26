using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using GameBase.Log;
using Managers;
using Sirenix.Serialization;
using UnityEngine;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tools
{
    public static class FileUtils
    {
        public static byte[] ReadAllBytesNullable(string path)
        {
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public static bool TryDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }

        public enum SerializeType
        {
            OdinJson = 0,
            OdinBinary = 1,
            UnityJson = 2,
        }
        
        //序列化保存Json
        public static void SerializeSaveJson<T>(string path, T data, SerializationContext context = null, SerializeType format = SerializeType.OdinJson)
        {
            try
            {
                byte[] bytes = null;
                string json = null;
                switch (format)
                {
                    case SerializeType.OdinBinary:
                        bytes = SerializationUtility.SerializeValue<T>(data, DataFormat.Binary, context);
                        break;
                    case SerializeType.OdinJson:
                        bytes = SerializationUtility.SerializeValue<T>(data, DataFormat.JSON, context);
                        break;
                    case SerializeType.UnityJson:
                        json = JsonUtility.ToJson(data, true);
                        break;
                    default:
                        bytes = SerializationUtility.SerializeValue<T>(data, DataFormat.JSON, context);
                        break;
                }
                //保存文件
                var dirPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
#if UNITY_EDITOR
                if (UnityEditor.VersionControl.Provider.enabled)
                {
                    UnityEditor.VersionControl.Provider.Checkout(path, UnityEditor.VersionControl.CheckoutMode.Asset).Wait();
                }
#endif
                if(format == SerializeType.UnityJson) File.WriteAllText(path, json, Encoding.UTF8);
                else File.WriteAllBytes(path, bytes);
#if UNITY_EDITOR
                if(!Application.isPlaying) AssetDatabase.Refresh();
#endif
            }
            catch (Exception e)
            {
                LogUtils.Error($"SerializeSave Fail!! path={path} format={format}");
                Debug.LogException(e);
            }
        }

        private static T DeserializeLoadJsonSync_Internal<T>(string fullPath, byte[] bytes, string json, DeserializationContext context = null,
            SerializeType format = SerializeType.OdinJson)
        {
            try
            {;
                if (bytes != null && bytes.Length > 0 || !json.IsNullOrEmpty())
                {
                    switch (format)
                    {
                        case SerializeType.OdinBinary:
                            return SerializationUtility.DeserializeValue<T>(bytes, DataFormat.Binary, context);
                        case SerializeType.OdinJson:
                            return SerializationUtility.DeserializeValue<T>(bytes, DataFormat.JSON, context);
                        case SerializeType.UnityJson:
                            return JsonUtility.FromJson<T>(json);
                        default:
                            return SerializationUtility.DeserializeValue<T>(bytes, DataFormat.JSON, context);
                    }
                    
                }
                else
                {
                    LogUtils.Debug($"DeserializeLoad Load NoFile，path={fullPath} format={format}");
                    return default(T);
                }
            }
            catch (Exception e)
            {
                LogUtils.Error($"DeserializeLoad Parse Fail!! path={fullPath} format={format}");
                Debug.LogException(e);
                return default(T);
            }
        }
        
        //序列化读取Json
        public static async UniTask<T> DeserializeLoadJson<T>(string fullPath, DeserializationContext context = null, SerializeType format = SerializeType.OdinJson)
        {
            var bytes = format != SerializeType.UnityJson ? await ResourceManager.LoadBytes(fullPath) : null;
            var json = format == SerializeType.UnityJson ? await ResourceManager.LoadString(fullPath) : null;
            return DeserializeLoadJsonSync_Internal<T>(fullPath, bytes, json, context, format);
        }
        
        //注意：暂不支持真机上读streamingasset下的文件
        public static T DeserializeLoadJsonSyncNoSteamingAsset<T>(string fullPath, DeserializationContext context = null, SerializeType format = SerializeType.OdinJson)
        {
            var bytes = format != SerializeType.UnityJson ? File.ReadAllBytes(fullPath) : null;
            var json = format == SerializeType.UnityJson ? File.ReadAllText(fullPath) : null;
            return DeserializeLoadJsonSync_Internal<T>(fullPath, bytes,json, context, format);
        }
        
        public static List<string> GetResNameWithRelativePath(string fullPath, string searchPattern = "*.asset", SearchOption option = SearchOption.AllDirectories)
        {
            List<string> list = null;
#if UNITY_EDITOR
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo direction = new DirectoryInfo(fullPath);  
                FileInfo[] files = direction.GetFiles(searchPattern,option);
                list = new List<string>();
                for(int i = 0; i < files.Length; i++)
                {  
                    if (files[i].Name.EndsWith(".meta"))
                    {  
                        continue;  
                    }
                    var name = files[i].FullName;
                    name = name.Replace(direction.FullName, "");
                    name = name.Substring(0, name.Length - searchPattern.Length + 1);
                    name = name.Replace("\\", "/");
                    list.Add(name);
                }
            }
#endif
            return list;
        }
        
        public static List<string> GetResName(string fullPath, string searchPattern = "*.asset", SearchOption option = SearchOption.AllDirectories)
        {
            List<string> list = null;
#if UNITY_EDITOR
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo direction = new DirectoryInfo(fullPath);  
                FileInfo[] files = direction.GetFiles(searchPattern,option);
                list = new List<string>();
                for(int i = 0; i < files.Length; i++)
                {  
                    if (files[i].Name.EndsWith(".meta"))
                    {  
                        continue;  
                    }
                    var name = files[i].Name;
                    list.Add(name.Substring(0, name.Length - searchPattern.Length + 1));
                }
            }
#endif
            return list;
        }

        //检查文件是否存在文件夹，无则创建
        public static bool TryCreateFilePathDirectory(string filePath)
        {
            var directPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directPath))
            {
                Directory.CreateDirectory(directPath);
                return true;
            }

            return false;
        }
        
        /// 判断文件是否存在，大小写敏感
        public static bool FileExistsWithCaseSensitivity(string fileName)
        {
            if (false == File.Exists(fileName))
            {
                return false;
            }
            
            var di = new DirectoryInfo(fileName);
            var info = di.Parent.GetFileSystemInfos(di.Name);
            if (info.Length == 0)
            {
                //MAC上，大小写异常时数组为0
                return false;
            }
            var realName = info[0].Name;
            return fileName.Contains(realName);
        }
        
        public static string GetFileMD5(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            MD5 md5 = MD5.Create();
            byte[] md5Bytes = md5.ComputeHash(fs);
            fs.Close();
            string fileMD5 = BitConverter.ToString(md5Bytes).Replace("-", "").ToLower();
            return fileMD5;
        }

        public static long GetFileSize(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        public static string FormatFileSize(long size)
        {
            int num = 1024;
            if (size < num)
            {
                return ZString.Concat(size, "B");
            }
            else if (size < num * num)
            {
                return ZString.Format("{0:f2}K", size * 1f/num);
            }
            else if (size < num * num * num)
            {
                return ZString.Format("{0:f2}M", size * 1f/(num * num));
            }
            return ZString.Format("{0:f2}G", size * 1f/(num * num * num));
        }
        
        /// <summary>
        /// 使用Unity的JsonUtility加载Json文件
        /// </summary>
        public static async UniTask<T> DeserializeLoadJsonUtility<T>(string fullPath)
        {
            try
            {
                string str = await ResourceManager.LoadString(fullPath);
                if (!string.IsNullOrEmpty(str))
                {
                    return JsonUtility.FromJson<T>(str);
                }
                else
                {
                    LogUtils.Debug($"DeserializeLoadJsonUtility Load NoFile，path={fullPath}");
                    return default(T);
                }
            }
            catch (Exception)
            {
                LogUtils.Error($"DeserializeLoadJsonUtility Parse Fail!! path={fullPath}");
                return default(T);
            }

        }
        
        /// <summary>
        /// 根据字符串创建文件(UTF-8 )
        /// </summary>
        public static void CreateFilesData(string path, string fileDataStr)
        {
            FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
            UTF8Encoding utf8 = new UTF8Encoding(true);
            StreamWriter sw = new StreamWriter(file, utf8);
            sw.Write(fileDataStr);
            sw.Close();
            file.Close();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 查找在某个文件夹下的所有类型资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folder">工程中文件夹相对路径</param>
        /// <param name="result">返回搜索的结果</param>
        public static void FindAssetInFolder<T>(string folder, List<T> result) where T : UnityEngine.Object
        {
            if (result == null)
                result = new List<T>();
            result.Clear();

            //定位到指定文件夹
            if (!Directory.Exists(folder))
                return;
            var directory = new DirectoryInfo(folder);

            //查询该文件夹下的所有文件；
            var files = directory.GetFiles();
            int length = files.Length;
            for (int i = 0; i < length; i++)
            {
                var file = files[i];

                //跳过Unity的meta文件（后缀名为.meta）
                if (file.Extension.Contains("meta"))
                    continue;

                //根据路径直接拼出对应的文件的相对路径
                string path = $"{folder}/{file.Name}";
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    result.Add(asset);
            }
        }
#endif
    }
}