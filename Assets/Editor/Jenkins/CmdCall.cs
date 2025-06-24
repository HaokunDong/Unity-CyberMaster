using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using GameBase.Log;
using Hypergryph;


namespace Hypergryph
{
	public class CmdCall {
		public enum version { Dist, Dev }
		public delegate void VersionDelegate(version ret);
		public delegate bool ObjectDelegate(GameObject obj);

		public enum Encoding
		{
			ASCII,
			BigEndianUnicode,
			Default,
			Unicode,
			UTF32,
			UTF7,
			UTF8,
			UTF8BOM,
		}

		public static void SelectPublishVersion(VersionDelegate cb)
		{
			int ret = EditorUtility.DisplayDialogComplex("Version", "choose publish package version\n debug(change server, on/off tutorial)", "release", "debug", "cancel");
			if (ret == 0) {
				LogUtils.Debug("chose release version");
				cb(version.Dist);
			}
			else if(ret == 1 ){
				LogUtils.Debug("chose debug version");
				cb(version.Dev);
			}
			else if(ret == 2){
				return ;
			}
		}


		public static bool FilterFile(string path) {
			if (path.EndsWith(".meta") || path.EndsWith(".DS_Store") || path.Contains(".svn") || path.EndsWith(".manifest"))
				return false;
			return true;
		}

        private static string _errorContent = "";

        public static bool NormalExecute()
        {
            return string.IsNullOrEmpty(_errorContent);
        }
        /// <summary>
		///  先进入目录，再执行对应的脚本。并不是从外部执行整体路径
		/// </summary>
		public static void ExecuteCmd(string batPath, string batFileName, bool showWindow, string arguments="")
		{
            _errorContent = "";
			bool waitForCmd = true;
			string fullPath = batPath + "/" + batFileName;
	        LogUtils.Debug ("ExecuteCmd " + fullPath + " " + arguments);

	        Process process = new Process();
	#if UNITY_EDITOR_OSX
			showWindow = false;
			process.StartInfo.FileName = fullPath;
			process.StartInfo.WorkingDirectory = batPath;
			process.StartInfo.Arguments = arguments;
	#else
			process.StartInfo.FileName = batFileName;
			process.StartInfo.WorkingDirectory = batPath;
			process.StartInfo.Arguments = arguments;
	#endif
			bool redirect = !showWindow;

			process.StartInfo.CreateNoWindow = !showWindow;
			process.StartInfo.UseShellExecute = !redirect;
			process.StartInfo.RedirectStandardInput = redirect;
			process.StartInfo.RedirectStandardOutput = redirect;
			process.StartInfo.RedirectStandardError = redirect;

	#if UNITY_EDITOR_OSX
	        process.OutputDataReceived += new DataReceivedEventHandler(
	           (s, e) =>
	           {
	               LogUtils.Debug(e.Data);
                   if (e.Data.Contains("fatal error") ||
                       e.Data.Contains("undeclared identifier") ||
                       e.Data.Contains("cannot find symbol") ||
                       e.Data.Contains("no such module") ||
                       e.Data.Contains("duplicate symbol") ||
                       e.Data.Contains("undefined reference to") ||
                       e.Data.Contains("linker command failed with exit code") ||
                       e.Data.Contains("could not build module") ||
                       e.Data.Contains("expected expression") ||
                       e.Data.Contains("ambiguous reference to member"))
                   {
                       LogUtils.Error(e.Data);
                       _errorContent = e.Data;
                   }
	           }
	        );
	        process.ErrorDataReceived += new DataReceivedEventHandler(
	            (s, e) =>
	            {
	            LogUtils.Error(e.Data);
	            }
	        );

	        process.Start();
	        process.BeginOutputReadLine();
	        process.WaitForExit();
	#else
	        process.Start();
            
	        if (redirect)
	        {
	            if (waitForCmd)
	            {
	                string strRet = process.StandardOutput.ReadToEnd();
	                string strError = process.StandardError.ReadToEnd();
	                LogUtils.Debug("Try Print Process output");
	                if (!string.IsNullOrEmpty(strRet))
	                    LogUtils.Debug("ret " + strRet);
	                if (!string.IsNullOrEmpty(strError))
	                    LogUtils.Error("error " + strError);
	                process.WaitForExit();
	            }
	        }
	        else
	        {
	            process.WaitForExit();
	        }
	#endif
	    }


	    public static Encoding GetFileEncode(string filename)
		{
			byte[] bytes = SysUtils.ReadBytes(filename);
	        if (bytes.Length == 0)
	        {
	            LogUtils.Error("filename, file size is 0: " + filename);
	            return Encoding.Default;
	        }
	        //LogUtils.Debug(Util.GetBytesHexString(bytes, bytes.Length, " "));
	        if (bytes[0] >= 0xEF)
			{
				if (bytes[0]==0xEF && bytes[1]==0xBB)
				{
					if (bytes[2] == 0xBF)
						return Encoding.UTF8BOM;
					return Encoding.UTF8;
				}
				else if(bytes[0]==0xFE && bytes[1]==0xFF)
				{
					return Encoding.BigEndianUnicode;
				}
				else if(bytes[0]==0xFF && bytes[1]==0xFE)
				{
					return Encoding.Unicode;
				}
				else
				{
					return Encoding.Default;
				}
			}
			else
			{
				int index = 0;
				string content = SysUtils.Read(filename);
				int len = content.Length;
				bool hasError = false;
				for (int i = 0; i < len; i++)
				{
					string substr = content.Substring(i, 1);
					byte[] cbytes = System.Text.Encoding.UTF8.GetBytes(substr);
					int slen = cbytes.Length;
					if (slen == 1) {
						index++;
						continue;
					}
					else if (slen == 2) {
						index += 2;
						continue;
					}
					else if (slen == 3) {
						if (cbytes[0] == bytes[index] && cbytes[1] == bytes[index + 1] && cbytes[2] == bytes[index + 2])
							return Encoding.UTF8;
						else
							return Encoding.ASCII;
					}
					else {
						hasError = true;
						LogUtils.Error(string.Format("{0}  invalid Char length:{1} index:{2}", filename, slen, i));
					}
				}
				return hasError ? Encoding.Default : Encoding.UTF8;
			}
		}
	}
}

