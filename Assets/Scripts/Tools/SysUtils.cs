using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Everlasting.Extend;
using GameBase.Log;
using GameBase.ObjectPool;
using Managers;
using Tools;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Griffin
{
	public class SysUtils
	{
        /// <summary>
		///  获取2021_03_18_12_34_03 这样的文本描述
		/// </summary>
		/// <returns></returns>
		public static string GetDateDesc()
		{
			return DateTime.Now.ToString("yyMMdd_HHmm");
		}

		public delegate bool FilterFunc(string path);

		public static string Read(string fileName) {
			string contents = null;
			try {
				contents = File.ReadAllText(fileName, Encoding.UTF8);
			}
			catch (Exception e) {
				LogUtils.Error(e.ToString());
			}
			return contents;
		}

		public static byte[] ReadBytes(string fileName) {
			byte[] bytes = null;
			try {
				bytes = File.ReadAllBytes(fileName);
			}
			catch (Exception e) {
				LogUtils.Error(e.ToString());
			}
			return bytes;
		}

		private static bool _Write(string fileName, byte[] bytes, FileMode mode=FileMode.OpenOrCreate) {
			try {
				if (bytes == null)
					return false;

				if (!MakeSureDirExist(Path.GetDirectoryName(fileName)))
					return false;

				using (FileStream fs = new FileStream(fileName, mode)) {
					fs.Write(bytes, 0, bytes.Length);
				}
			}
			catch (Exception e) {
				LogUtils.Warning(e.ToString());
				return false;
			}
			return true;
		}

		/// <summary>
		/// 写入普通文件，可以使用这个接口.重复使用时，请参考_WriteToFile
		/// </summary>
		public static bool Write(string fileName, string contents, FileMode mode=FileMode.OpenOrCreate) {
			if (contents == null)
				return false;
			byte[] bytes = Encoding.UTF8.GetBytes(contents);
			return _Write(fileName, bytes, mode);
		}

		public static bool CopyDirectory(string srcDir, string destDir, FilterFunc filter=null, FilterFunc processor=null) {
			if (!Directory.Exists(srcDir) || (filter != null && !filter(srcDir)))
				return false;

			if (!CopyFiles(srcDir, destDir, filter, processor))
				return false;

			string[] srcSubDirs = Directory.GetDirectories (srcDir);
			foreach (string srcSubDir in srcSubDirs) {
				string destSubDir = destDir + srcSubDir.Substring(srcDir.Length);
				if (!CopyDirectory(srcSubDir, destSubDir, filter, processor))
					return false;
			}
			return true;
		}


		public static bool CopyFiles(string srcDir, string destDir, FilterFunc filter=null, FilterFunc processor=null) {
			if (!MakeSureDirExist(destDir))
				return false;

			try {
				string destFilePath = null;
				string[] fileName = Directory.GetFiles(srcDir);
				foreach (string filePath in fileName) {
					if (filter == null || filter(filePath)) {
						destFilePath = destDir + filePath.Substring(srcDir.Length);
						_ExeSetWritable(filePath);
						if (FileExists(destFilePath))
						{
							_ExeSetWritable(destFilePath);
						}
						File.Copy(filePath, destFilePath, true);
						if (processor != null)
							processor(destFilePath);
					}
				}
			}
			catch (Exception ex) {
				LogUtils.Warning(ex.ToString());
				return false;
			}
			return true;
		}

		/// <summary>
		/// 移动文件夹。 Available since 1.1
		/// </summary>
		public static bool MoveDirectory(string srcDir, string destDir, FilterFunc filter=null, FilterFunc processor=null) {
			if (!Directory.Exists(srcDir) || (filter != null && !filter(srcDir)))
				return false;

			if (!MoveFiles(srcDir, destDir, filter, processor))
				return false;

			string[] srcSubDirs = Directory.GetDirectories (srcDir);
			foreach (string srcSubDir in srcSubDirs) {
				string destSubDir = destDir + srcSubDir.Substring(srcDir.Length);
				if (!MoveDirectory(srcSubDir, destSubDir, filter, processor))
					return false;
			}
			return true;
		}

		/// <summary>
		/// 移动多个文件。 Available since 1.1
		/// </summary>
		public static bool MoveFiles(string srcDir, string destDir, FilterFunc filter=null, FilterFunc processor=null) {
			if (!MakeSureDirExist(destDir))
				return false;

			try {
				string destFilePath = null;
				string[] fileName = Directory.GetFiles(srcDir, "*.*", SearchOption.AllDirectories);
				foreach (string filePath in fileName) {
					if (filter == null || filter(filePath))
					{
						var unifiedPath = filePath.Replace("\\", "/");
						int index = unifiedPath.LastIndexOf("/");
						unifiedPath = unifiedPath.Substring(index);
						destFilePath = destDir + unifiedPath;
						DeleteFile(destFilePath);
						File.Move(filePath, destFilePath);

						if (processor != null)
							processor(destFilePath);
					}
				}
			}
			catch (Exception ex) {
				LogUtils.Warning(ex.ToString());
				return false;
			}
			return true;
		}
		
		public static void ChangeLayer(Transform trans, int layer)
		{
			trans.gameObject.layer = layer;
			foreach (Transform child in trans)
			{
				ChangeLayer(child, layer);
			}
		}


		public static void RenameFiles(string dir, string srcName, string destName, FilterFunc filter=null) {
			if (!Directory.Exists(dir) || !filter(dir))
				return;

			string destPath = null;
			string[] fileName = Directory.GetFiles(dir);
			foreach (string filePath in fileName) {
				if (filter(filePath)) {
					destPath = filePath.Replace(srcName, destName);
					File.Move(filePath, destPath);
				}
			}

			string[] srcSubDirs = Directory.GetDirectories(dir);
			foreach (string srcSubDir in srcSubDirs) {
				RenameFiles(srcSubDir, srcName, destName, filter);
			}
		}

		public static void MoveFile(string filePath, string destPath)
		{
			File.Move(filePath, destPath);
		}

		public static bool MakeSureDirExist(string dir) {
			bool ret = true;
			if (!Directory.Exists(dir)) {
				try {
					Directory.CreateDirectory(dir);
				}
				catch (Exception ex) {
					// maybe disk full
					ret = false;
					LogUtils.Warning(ex.ToString());
				}
			}
			return ret;
		}

		public static bool ClearDirectory(string dir) {
			if (!Directory.Exists(dir))
				return false;

			string[] subFiles = Directory.GetFiles(dir);
			int count = subFiles.Length;
			for (int i = 0; i < count; i++) {
				DeleteFile(subFiles[i]);
			}

			string[] subDirs = Directory.GetDirectories(dir);
			int len = subDirs.Length;
			for (int i = 0; i < len; i++) {
				DeleteDirectory(subDirs[i]);
			}
			return true;
		}

		public static bool FileExists(string path) {
			return File.Exists(path);
		}

		public static bool DirectoryExists(string path) {
			return Directory.Exists(path);
		}

		public static bool DeleteFile(string path)
		{
#if UNITY_EDITOR
			if (File.Exists(path))
			{
				if (UnityEditor.VersionControl.Provider.enabled)
				{
					UnityEditor.VersionControl.Provider.Delete(path).Wait();
				}
			}
#endif
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			return true;
		}

		public static bool DeleteDirectory(string path)
		{
#if UNITY_EDITOR
			if (Directory.Exists(path))
			{
				if (UnityEditor.VersionControl.Provider.enabled)
				{
					UnityEditor.VersionControl.Provider.Delete(path).Wait();
				}
			}
#endif
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
			return true;
		}

		public static bool IsDir(string path) {
			//FileAttributes attr = File.GetAttributes(path);
			//bool isDir = (attr & FileAttributes.Directory) == FileAttributes.Directory;
			bool isDir = Directory.Exists(path);
			return isDir;
		}

		/// <summary>
		/// 读取path路径下所有文件。 Available since 1.1
		/// </summary>
		public static string[] GetAllFiles(string path, string filter, SearchOption option) {
			string[] files = Directory.GetFiles(path, filter, option);
			return files;
		}

		public static bool FilterFile(string path) {
			if (path.EndsWithEx(".meta") || path.EndsWithEx(".DS_Store")
			                             || path.Contains(".svn") || path.EndsWithEx(".manifest"))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// return absolute path from an asset path, such as "Assets/Content/A.jpg" => "E:/Tina/Assets/Content/A.jpg"
		/// </summary>
		/// <param name="assetPath">path begin with "Assets/"</param>
		/// <returns></returns>
		public static string AssetPathToAbsPath(string assetPath)
		{
			string absPath = null;
			var sb = StaticPool<StringBuilder>.Get();
			sb.Clear();
			sb.Append(Application.dataPath);

			//skip "Assets" or "/Assets" or "\Assets"
			if (assetPath.StartsWith(@"/") || assetPath.StartsWith(@"\"))
				sb.Append(assetPath.Substring(7));
			else
				sb.Append(assetPath.Substring(6));

			sb.Replace(@"\", @"/");
			sb.Replace(@"//", @"/");
			absPath = sb.ToString();
			StaticPool<StringBuilder>.Return(sb);
			return absPath;
		}

		/// <summary>
		/// 设置文件、文件夹可写
		/// </summary>
		public static bool SetFileWritable(string filePath, bool recursive = true)
		{
			string fullPath = filePath;
			//convert assetPath to absolute path
			if (fullPath.StartsWith("Assets/"))
				fullPath = AssetPathToAbsPath(fullPath);

			try
			{
				_ExeSetWritable(fullPath);
				if(Directory.Exists(fullPath) && recursive)
				{
					foreach(var f in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
					{
						_ExeSetWritable(f);
					}

					foreach (var f in Directory.GetDirectories(fullPath, "*", SearchOption.AllDirectories))
					{
						_ExeSetWritable(f);
					}
				}
				return true;
			}
			catch (System.Exception e)
			{
				Debug.LogErrorFormat("Fail to set file({0}) be writable, for the reason of {1}", filePath, e.Message);
				return false;
			}
		}

		private static void _ExeSetWritable(string path)
		{
			FileAttributes fa = File.GetAttributes(path);
			if (fa.HasFlag(FileAttributes.ReadOnly))
			{
				File.SetAttributes(path, System.IO.FileAttributes.Normal);
			}
		}


		/// <summary>
        /// read text from a file on device
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadText(string filePath)
        {
            string txt = "";
#if UNITY_ANDROID && !UNITY_EDITOR
            txt = _ReadTextOnAndroid(filePath);
#else
            try
            {
                txt = File.ReadAllText(filePath);
            }
            catch (System.Exception e)
            {
                LogUtils.Error(string.Format("ReadAllText:{0} failed for the reason:{1}", filePath, e.Message));
            }
#endif
            return txt;
        }


        /// <summary>
        /// read text from file on android platform, especially on jar file like Application.streamingAssetsPath
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string _ReadTextOnAndroid(string filePath)
        {
            byte[] bytes = _LoadFileOnAndroid(filePath);
            if (bytes == null)
                return null;
            string txt = System.Text.UTF8Encoding.UTF8.GetString(bytes);
            return txt;
        }

        /// <summary>
        /// load file data(byte[]) on android platform
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static byte[] _LoadFileOnAndroid(string filePath)
        {
            if (filePath.Contains("jar:"))
                return LoadFileWithWWW(filePath);
            else
                return LoadFileWithIO(filePath);
        }

        public static byte[] LoadFileWithWWW(string filePath)
        {
            LogUtils.Debug(string.Format("LoadFileWithWWW: {0}", filePath));
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            www.timeout = 2;
            www.SendWebRequest();
            while (!www.isDone)
            {
                //do nothing
            }
            if (www.downloadHandler == null || !string.IsNullOrEmpty(www.error) || www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
            {
                LogUtils.Debug(string.Format("LoadFileWithWWW({0}) Failed! Error Msg:{1}", filePath, www.error));
                return null;
            }

            byte[] bytes = www.downloadHandler.data;
            return bytes;
        }

        public static byte[] LoadFileWithIO(string filePath)
        {
            byte[] bytes = null;
            try
            {
                bytes = File.ReadAllBytes(filePath);
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("ReadAllBytes:{0} failed for the reason:{1}", filePath, e.Message);
            }
            return bytes;
        }

        /// <summary>
        /// 检查Component是否存在，并返回已存在或新建的Component
        /// </summary>
        public static T MakeComponentExist<T>(GameObject curObj) where T : Component
        {
	        T cur = curObj.GetComponent<T>();
	        if (cur == null)
	        {
		        cur = curObj.AddComponent<T>();
	        }
	        return cur;
        }

        //考虑半径，将坐标计算出来
        public static Vector3 GetPosWithRadius(Vector3 PosFrom, Vector3 PosEnd, float radius)
        {
	        return PosEnd + (PosFrom - PosEnd).normalized * radius;
        }

        public static T Clone<T>(T obj)
        {
	        var inst = obj.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

	        return (T)inst?.Invoke(obj, null);
        }

        /// <summary>
        /// 获取包括子节点下的所有Component
        /// </summary>
        public static List<T> GetComponents<T>(Transform tran)
        {
	        List<T> componentList = new List<T>();
	        _GetRecursiveCom(tran, ref componentList);
	        return componentList;
        }

        private static void _GetRecursiveCom<T>(Transform parent, ref List<T> componentList)
        {
	        var comps = parent.GetComponents<T>();
	        if (comps != null && comps.Length != 0)
	        {
		        foreach (var VARIABLE in comps)
		        {
			        componentList.Add(VARIABLE);
		        }
	        }

	        for (int i = 0; i < parent.childCount; i++)
	        {
		        _GetRecursiveCom(parent.GetChild(i), ref componentList);
	        }
        }

        /// <summary>
        /// 移除最后一个，并返回移除的值(防止List内部移位)
        /// </summary>
        public static T RemoveAt<T>(List<T> curList)
        {
	        var cur = curList[curList.Count - 1];
	        curList.RemoveAt(curList.Count - 1);
	        return cur;
        }
        
        public static void AddToP4PendingIfEnabled(string relativePath)
        {
            #if UNITY_EDITOR
            if (UnityEditor.VersionControl.Provider.enabled)
            {
                UnityEditor.VersionControl.Provider.Checkout(relativePath, UnityEditor.VersionControl.CheckoutMode.Asset)
                    .Wait();
            }
            #endif
        }
        
        public static void ExpandTab(ref string prefixBlank)
        {
            prefixBlank = prefixBlank + "    ";
        }

        public static void ShrinkTab(ref string preBlank)
        {
            preBlank = preBlank.Substring(0, preBlank.Length - 4);
        }
	}
}
