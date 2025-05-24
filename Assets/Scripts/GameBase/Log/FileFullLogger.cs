// using System;
// using System.IO;
// using UnityEngine;
//
// namespace GameBase.Log
// {
//     //同步写完整日志，用于查BUG
//     //有Editor.log脚本，一般来说用不上
//     public class FileFullLogger : IDisposable
//     {
//         private static StreamWriter s_sw = null;
//         public FileFullLogger()
//         {
//             Application.logMessageReceived += LogCallback;
//             var path = Path.Combine(Application.persistentDataPath, $"unity_full_log.log");
//             s_sw = File.CreateText(path);
//         }
//
//         ~FileFullLogger()
//         {
//             Dispose(false);
//         }
//         
//         private void LogCallback(string condition, string stackTrace, LogType type)
//         {
//             s_sw.Write($"[{type.ToString()}]");
//             s_sw.WriteLine(condition);
//             s_sw.WriteLine(stackTrace);
//             s_sw.Flush();
//         }
//         
//         public void Dispose()
//         {
//             Dispose(true);
//             GC.SuppressFinalize(this);
//         }
//         
//         protected void Dispose(bool disposing)
//         {
//             s_sw.Close();
//             s_sw = null;
//             Application.logMessageReceived -= LogCallback;
//         }
//     }
// }