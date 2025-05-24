using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace GameBase.Log
{
    //多线程写文件
    internal class FileLogger : ILogger
    {
        private struct LogEntry
        {
            public string time;
            public object log;
            public LogUtils.LogType logType;

            public LogEntry(object log, LogUtils.LogType logType, string time)
            {
                this.log = log;
                this.logType = logType;
                this.time = time;
            }
        }
        
        private StreamWriter m_sw = null;
        public string LogPath { get; private set; }
        
        private Channel<LogEntry> channel;
        private Task m_writeLoop;
        private CancellationTokenSource m_cancellationTokenSource;
        public long WriteStringLength { get; private set; }

        internal FileLogger()
        {
            var now = DateTime.Now;
            var t = $"{now.Month}-{now.Day}_{now.Hour}-{now.Minute}-{now.Millisecond}";
            var path = Path.Combine(Application.persistentDataPath, $"unity_{t}.log");
            if (File.Exists(path))
            {
                for (int i = 1; i < 100; i++)
                {
                    path = Path.Combine(Application.persistentDataPath, $"unity_{t}_{i}.log");
                    if(!File.Exists(path)) break;
                }
            }
            Init(path);
        }
        
        internal FileLogger(string path)
        {
            Init(path);
        }

        private void Init(string path)
        {
            try
            {
                m_sw = File.CreateText(path);
                LogPath = path;
            }
            catch (IOException e)
            {
                Debug.Log("CreateRollingFile IOWrong path:" + path + "TryNewFile");
                //若冲突，不同文件名尝试10次
                for (int i = 1; i < 10; i++)
                {
                    try
                    {
                        var ext = Path.GetExtension(path);
                        var newPath = $"{path.Substring(0, path.Length - ext.Length)}_{i}{ext}";
                        m_sw = File.CreateText(newPath);
                        LogPath = newPath;
                        path = newPath;
                        break;
                    }
                    catch (IOException e2)
                    {
                        
                    }
                }

                if (m_sw == null) throw;
            }
            
            Debug.Log("Create Log File: " + path); 
                        
            this.m_cancellationTokenSource = new CancellationTokenSource();
            this.channel = Channel.CreateSingleConsumerUnbounded<LogEntry>();
            this.m_writeLoop = Task.Run(WriteLoop);
        }

        ~FileLogger() => Dispose(false);

        private string m_timeStr;
        private float m_lastBuildTimeStr = -1f;
        
        private string BuildTimeStr()
        {
            if (Time.time - m_lastBuildTimeStr < 1) return m_timeStr;
            using (var sb = new Cysharp.Text.Utf16ValueStringBuilder(true))
            {
                DateTime now = DateTime.Now;
                sb.Append(now.Year); sb.Append('-');
                sb.Append(now.Month); sb.Append('-');
                sb.Append(now.Day); sb.Append(' ');
                sb.Append(now.Hour); sb.Append(':');
                sb.Append(now.Minute); sb.Append(':');
                sb.Append(now.Second); sb.Append('.');
                sb.Append(now.Millisecond); sb.Append(' ');
                sb.Append(Time.time);
                m_timeStr = sb.ToString();
                m_lastBuildTimeStr = Time.time;
                return m_timeStr;
            }
        }
        
        public void Print(object log, LogUtils.LogType logType, Object context)
        {
            channel.Writer.TryWrite(new LogEntry(log, logType, BuildTimeStr()));
        }

        public void Assert(string log, Object context)
        {
            channel.Writer.TryWrite(new LogEntry(log, LogUtils.LogType.Assert, BuildTimeStr()));
        }

        private async Task WriteLoop()
        {
            var reader = channel.Reader;
            try
            {
                while (await reader.WaitToReadAsync())
                {
                    try
                    {
                        while (reader.TryRead(out var entry))
                        {
                            string log = ZString.Format("[{0}][{1}]{2}", entry.time, entry.logType,  entry.log);
                            m_sw.WriteLine(log);
                            WriteStringLength += log.Length;
                        }

                        if (!m_cancellationTokenSource.IsCancellationRequested)
                        {
                            await Task.Delay(500, m_cancellationTokenSource.Token).ConfigureAwait(false);
                        }
                        m_sw.Flush(); // flush before wait.
                    }
                    catch (OperationCanceledException){}
                    catch (Exception ex)
                    {
                        try
                        {
                            Debug.LogError(ex);
                        }
                        catch { }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                try
                {
                    m_sw.Flush();
                }
                catch { }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            DelayDispose().AsTask().Wait();
        }

        private async ValueTask DelayDispose()
        {
            try
            {
                channel.Writer.Complete();
                m_cancellationTokenSource.Cancel();
                await m_writeLoop.ConfigureAwait(false);
            }
            finally
            {
                m_sw.Dispose();
                m_sw = null;
            }
        }
    }
}