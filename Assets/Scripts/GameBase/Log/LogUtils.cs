using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace GameBase.Log
{
     public static class LogUtils
    {
        //通过Console界面上筛选，关闭的Channel会不再打印
        [Flags]
        public enum LogType
        {
            Trace = 0x01,
            Debug = 0x02,
            Warning = 0x04,
            Assert = 0x08,
            Error = 0x10,
            Exception = 0x20,
            InitShow = Debug | Warning | Assert | Error | Exception,
        }

        public static bool IsLogTypeEnable(LogType type)
        {
            return (type & s_logTypeEnableFlag) > 0;
        }

        private static readonly List<ILogger> loggerList = new List<ILogger>();
        //即使不打印trace时，这个也会在文件记录，utf8为1-6字节 大小大约限制在1M左右
        private static ILogger s_traceRollingFileLogger;

        private static LogType s_logTypeEnableFlag;
        //是否打印Trace调试信息
        public static bool IsLogTraceOn
        {
            get => IsLogTypeEnable(LogType.Trace);
            set
            {
                if (IsLogTraceOn != value)
                {
                    if(value) s_logTypeEnableFlag |= LogType.Trace;
                    else s_logTypeEnableFlag &= ~LogType.Trace;
#if UNITY_EDITOR
                    UnityEditor.EditorPrefs.SetBool("LoggerSetting_IsLogTraceOn", IsLogTraceOn);
#endif
                }
            }
        }

        //error不受影响
        private static LogChannel s_showLogChannel;

        public static LogChannel LogChannel
        {
            get => s_showLogChannel;
            set
            {
                if (s_showLogChannel != value)
                {
                    s_showLogChannel = value;
#if UNITY_EDITOR
                    UnityEditor.EditorPrefs.SetInt("LoggerSetting_LogChannel", (int)s_showLogChannel);
#endif
                }
            }
        }
        
        //log总开关，error不受影响
        public static bool logEnable = true;

        public static void SetLogChannelEnable(LogChannel channel, bool isEnable)
        {
            _ = isEnable ? LogChannel |= channel : LogChannel &= ~channel;
        }
        
        public static bool IsLogChannelEnable(LogChannel channel)
        {
            return logEnable && (LogChannel & channel) > 0;
        }

        private static bool s_printTimeFrame;

        public static bool PrintTimeFrame
        {
            get => s_printTimeFrame;
            set
            {
                if (s_printTimeFrame != value)
                {
                    s_printTimeFrame = value;
#if UNITY_EDITOR
                    UnityEditor.EditorPrefs.SetBool("LoggerSetting_PrintTimeFrame", s_printTimeFrame);
#endif
                }
            }
        }
        
        static LogUtils()
        {
            s_logTypeEnableFlag = LogType.InitShow;
            s_showLogChannel = LogChannel.AllNoMessage;
#if UNITY_EDITOR
            var logChannelFromPrefs = EditorPrefs.GetInt("LoggerSetting_LogChannel");
            if(logChannelFromPrefs != 0) s_showLogChannel = (LogChannel)logChannelFromPrefs;
            IsLogTraceOn = EditorPrefs.GetBool("LoggerSetting_IsLogTraceOn");
            s_printTimeFrame = EditorPrefs.GetBool("LoggerSetting_PrintTimeFrame");
            loggerList.Add(new UnityLogger());
            if (Application.isPlaying)
            {
                s_traceRollingFileLogger = new RollingFileLogger(1024 * 1024);
            }
#else
            loggerList.Add(new UnityLogger());
            //loggerList.Add(new FileLogger());
#endif
            GRNLogUtils.RegisterDelegates(
                (log, channel, color, context)=>{Trace(log, (LogChannel)channel, color, context);},
                (log, channel, color, context) => { Debug(log, (LogChannel) channel, color, context); },
                (log, channel, color, context) => { Warning(log, (LogChannel) channel, color, context); },
                (log, channel, color, context) => { Error(log, (LogChannel) channel, color, context); });
        }

        public static void AddFileLogger()
        {
            loggerList.Add(new FileLogger());
        }

        //级别比Debug低，默认不显示（编辑器通过Console界面里设置开启，真机可通过调试面板-GM命令-开关LogTrace），用于比较频繁的消息
#if !UNITY_EDITOR && (LOG_DISABLE || LOG_TRACE_DISABLE)
        [Conditional("FALSE")]
#endif
        public static void Trace(object log, LogChannel channel, Color? color = null, Object context = null)
        {
            // Profiler.BeginSample("Trace_Log_Cost");
            if(color == null) color = Color.white;
            PrintLog(log, LogType.Trace, channel, color, context);
            // Profiler.EndSample();
        }

#if !UNITY_EDITOR && LOG_DISABLE
        [Conditional("FALSE")]
#endif
        public static void Debug(object log, LogChannel channel = LogChannel.Common, Color? color = null, Object context = null)
        {
            PrintLog(log, LogType.Debug, channel, color, context);
        }

#if !UNITY_EDITOR && LOG_DISABLE
        [Conditional("FALSE")]
#endif
        public static void Warning(object log, LogChannel channel = LogChannel.Common, Color? color = null, Object context = null)
        {
            PrintLog(log, LogType.Warning, channel, color, context);
        }

#if !UNITY_EDITOR && LOG_DISABLE
        [Conditional("FALSE")]
#endif
        public static void Error(object log, LogChannel channel = LogChannel.Common, Color? color = null, Object context = null)
        {
            PrintLog(log, LogType.Error, channel, color, context);
        }

        private static void PrintLog(object log, LogType logType, LogChannel channel, Color? color, Object context)
        {
            // Profiler.BeginSample("Log_Cost"); 绝大部分开销都是unity的log系统贡献，大部分开销在追溯堆栈上
            //Error不屏蔽Channel
            if (!IsLogChannelEnable(channel) && (logType != LogType.Error && logType != LogType.Exception)) return;
            string buildLog = null;
            if (s_traceRollingFileLogger != null)
            {
                buildLog = BuildLog(log, logType, color, channel);
                s_traceRollingFileLogger.Print(buildLog, logType, context);
            }
            if(!IsLogTypeEnable(logType)) return;
            buildLog ??= BuildLog(log, logType, color, channel);
            foreach (var logger in loggerList)
            {
                logger.Print(buildLog, logType, context);
            }
            // Profiler.EndSample();
        }
        
#if !UNITY_EDITOR && LOG_DISABLE
        [Conditional("FALSE")]
#endif
        public static void Assert(bool condition, string message, LogChannel channel = LogChannel.Common, Color? color = null, Object context = null)
        {
            if (condition) return;
            PrintAssert(false, message, channel, color, context);
        }
        
        private static void PrintAssert(bool condition, string message, LogChannel channel, Color? color, Object context)
        {
            if (!IsLogChannelEnable(channel)) return;
            string buildLog = null;
            if (s_traceRollingFileLogger != null)
            {
                Profiler.BeginSample("RollingFileLogger_Cost");
                buildLog = BuildLog(message, LogType.Assert, color, channel);
                s_traceRollingFileLogger.Print(buildLog, LogType.Assert, context);
                Profiler.EndSample();
            }
            if(!IsLogTypeEnable(LogType.Assert)) return;
            buildLog ??= BuildLog(message, LogType.Assert, color, channel);
            foreach (var logger in loggerList)
            {
                logger.Assert(buildLog, context);
            }
        }

        private static Dictionary<LogChannel, string> s_logChannel2StrDic = new Dictionary<LogChannel, string>();
        private static string LogChannel2Str(LogChannel logChannel)
        {
            return s_logChannel2StrDic.TryGetValue(logChannel, out string result) ? result : s_logChannel2StrDic[logChannel] = logChannel.ToString();
        }
        private static string BuildLog(object message, LogType logType, Color? color, LogChannel logChannel)
        {
            using (var sb = new Cysharp.Text.Utf16ValueStringBuilder(true))
            {
                if (color != null)
                {
                    sb.Append("<color=#");
                    Color32 color32 = color.Value;
                    sb.Append(color32.r, "X2");
                    sb.Append(color32.g, "X2");
                    sb.Append(color32.b, "X2");
                    sb.Append(color32.a, "X2");
                    sb.Append(">");
                }

                if (PrintTimeFrame)
                {
                    sb.Append(Time.frameCount);
                    sb.Append(' ');
                }

                if(logChannel != LogChannel.Common)
                {
                    sb.Append('[');
                    sb.Append(LogChannel2Str(logChannel));
                    sb.Append(']');
                    sb.Append(' ');
                }
                
                sb.Append(message);
            
                if (color != null)
                {
                    sb.Append("</color>");
                }

                return sb.ToString();
            }
        }
    }
}