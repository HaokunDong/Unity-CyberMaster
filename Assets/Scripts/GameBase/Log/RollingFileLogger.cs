using System;
using System.IO;
using Everlasting.Extend;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameBase.Log
{
    public class RollingFileLogger : ILogger
    {
        private long m_maxStringLength;
        private FileLogger m_fileLogger;
        public static readonly string LOGFilePath = Application.persistentDataPath + "/unity_rolling_log.log";
        
        internal RollingFileLogger(long maxStringLength)
        {
            m_fileLogger = new FileLogger(LOGFilePath);
            m_maxStringLength = maxStringLength;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            m_fileLogger?.Dispose();
            m_fileLogger = null;
        }

        public void Print(object log, LogUtils.LogType logType, Object context)
        {
            m_fileLogger.Print(log, logType, context);
            CheckLogMaxLength();
        }

        public void Assert(string log, Object context)
        {
            m_fileLogger.Assert(log, context);
            CheckLogMaxLength();
        }

        private void CheckLogMaxLength()
        {
            if (m_fileLogger.WriteStringLength > m_maxStringLength)
            {
                string filePath = m_fileLogger.LogPath;
                m_fileLogger.Dispose();
                m_fileLogger = null;
                try
                {
                    var backLogFilePath = filePath.AddTailFileName("2");
                    if(File.Exists(backLogFilePath)) File.Delete(backLogFilePath);
                    File.Move(filePath, backLogFilePath);
                }
                catch (IOException e)
                {
                    Debug.LogException(e);
                }
                
                m_fileLogger = new FileLogger(filePath);
            }
        }
    }
}