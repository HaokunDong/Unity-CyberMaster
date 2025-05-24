using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace GameBase.Log
{
    public interface ILogger : IDisposable
    {
        void Print(object log, LogUtils.LogType logType, Object context);
        void Assert(string log, Object context);
    }

    internal class UnityLogger : ILogger
    {
        public void Print(object log, LogUtils.LogType logType, Object context)
        {
            switch (logType)
            {
                case LogUtils.LogType.Trace:
                    Debug.Log(log, context);
                    break;
                case LogUtils.LogType.Debug:
                    Debug.Log(log, context);
                    break;
                case LogUtils.LogType.Warning:
                    Debug.LogWarning(log, context);
                    break;
                case LogUtils.LogType.Error:
                case LogUtils.LogType.Exception:
                    Debug.LogError(log, context);
                    break;
            }
        }

        public void Assert(string log, Object context)
        {
            Debug.Assert(true, log, context);
        }

        public void Dispose()
        {
            
        }
    }
}