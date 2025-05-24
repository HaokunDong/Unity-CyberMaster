using UnityEngine;

public static class GRNLogUtils
{
    public delegate void LogDelegate(object log, int channel = 0x0040, Color? color = null, Object context = null);
    //由项目Log系统调用
    public static void RegisterDelegates(LogDelegate traceDelegate, LogDelegate debugDelegate, LogDelegate warningDelegate, LogDelegate errorDelegate)
    {
        s_traceDelegate = traceDelegate;
        s_debugDelegate = debugDelegate;
        s_warningDelegate = warningDelegate;
        s_errorDelegate = errorDelegate;
    }

    private static LogDelegate s_traceDelegate;
#if !UNITY_EDITOR && (LOG_DISABLE || LOG_TRACE_DISABLE)
        [Conditional("FALSE")]
#endif
    public static void Trace(object log, int channel = 0x0040, Color? color = null, Object context = null)
    {
        if (s_traceDelegate != null) s_traceDelegate(log, channel, color, context);
        else UnityEngine.Debug.Log(log, context);
    }

    private static LogDelegate s_debugDelegate;
#if !UNITY_EDITOR && LOG_DISABLE
        [Conditional("FALSE")]
#endif
    public static void Debug(object log, int channel = 0x0040, Color? color = null, Object context = null)
    {
        if (s_traceDelegate != null) s_debugDelegate(log, channel, color, context);
        else UnityEngine.Debug.Log(log, context);
    }

    private static LogDelegate s_warningDelegate;
#if !UNITY_EDITOR && LOG_DISABLE
        [Conditional("FALSE")]
#endif
    public static void Warning(object log, int channel = 0x0040, Color? color = null, Object context = null)
    {
        if (s_traceDelegate != null) s_warningDelegate(log, channel, color, context);
        else UnityEngine.Debug.LogWarning(log, context);
    }

    private static LogDelegate s_errorDelegate;
#if !UNITY_EDITOR && LOG_DISABLE
        [Conditional("FALSE")]
#endif
    public static void Error(object log, int channel = 0x0040, Color? color = null, Object context = null)
    {
        if (s_traceDelegate != null) s_errorDelegate(log, channel, color, context);
        else UnityEngine.Debug.LogError(log, context);
    }
}