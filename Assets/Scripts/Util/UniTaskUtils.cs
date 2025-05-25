using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using GameBase.Log;
using GameBase.ObjectPool;

namespace Tools
{
    public static class UniTaskUtils
    {
        public class ActionWithUniTaskCompletionSource
        {
            public AutoResetUniTaskCompletionSource source;
            public UniTask Task => source.Task;

            // private static int sourceIdTotal = 0;
            // private static Dictionary<AutoResetUniTaskCompletionSource, int> source2ID =
            //     new Dictionary<AutoResetUniTaskCompletionSource, int>();

            // public static int TryGetId(AutoResetUniTaskCompletionSource source)
            // {
            //     if (source2ID.TryGetValue(source, out int value))
            //     {
            //         return value;
            //     }
            //
            //     source2ID[source] = ++sourceIdTotal;
            //     return sourceIdTotal;
            // }

            public void Init()
            {
                source ??= AutoResetUniTaskCompletionSource.Create();
                // LogUtils.Error($"ActionWithUniTaskCompletionSource Init SourceId:{TryGetId(source)} threadId:{Thread.CurrentThread.ManagedThreadId}");
                
            }

#if UNITY_EDITOR
            private string m_recordStackTrace;
#endif
            
            public void Complete()
            {
                try
                {
                    if (source != null)
                    {
                        // int sourceId = TryGetId(source);
                        // LogUtils.Error($"ActionWithUniTaskCompletionSource Complete Before SourceId:{sourceId}");
#if UNITY_EDITOR
                        m_recordStackTrace = new StackTrace(0,true).ToString();   
#endif
                        TrySetResultAndSafeSetNull(ref source);
                        
                        // LogUtils.Error($"ActionWithUniTaskCompletionSource Complete After SourceId:{sourceId}");
                    }
                    else
                    {
#if UNITY_EDITOR
                        LogUtils.Error($"UniTask WaitCallback被多次调用！检查下是否存在逻辑问题 记录堆栈={m_recordStackTrace}");
#else
                        LogUtils.Error($"UniTask WaitCallback被多次调用！检查下是否存在逻辑问题");
#endif
                    }
                    
                }
                finally
                {
                    StaticPool<ActionWithUniTaskCompletionSource>.Return(this);
                }
            }
        }
        
        private class ActionWithUniTaskCompletionSource<T>
        {
            public AutoResetUniTaskCompletionSource<T> source;
            public UniTask<T> Task => source.Task;
            public void Init()
            {
                source ??= AutoResetUniTaskCompletionSource<T>.Create();
            }
            public void Complete(T result)
            {
                try
                {
                    source.TrySetResult(result);
                }
                finally
                {
                    source = null;
                    StaticPool<ActionWithUniTaskCompletionSource<T>>.Return(this);
                }
            }
        }
        
        public static UniTask WaitCallback(out Action onComplete)
        {
            var waitSource = StaticPool<ActionWithUniTaskCompletionSource>.Get();
            // var waitSource = new ActionWithUniTaskCompletionSource();
            waitSource.Init();
            onComplete = waitSource.Complete;
            return waitSource.Task;
        }
        
        public static UniTask<T> WaitCallback<T>(out Action<T> onComplete)
        {
            var waitSource = StaticPool<ActionWithUniTaskCompletionSource<T>>.Get();
            waitSource.Init();
            onComplete = waitSource.Complete;
            return waitSource.Task;
        }

        //由于AutoResetUniTaskCompletionSource自带对象池机制，TrySetResult如果被调用多次，可能会完成别处的task，后果非常严重
        //尽量用这个代替原生的TrySetResult
        public static void TrySetResultAndSafeSetNull(ref AutoResetUniTaskCompletionSource source)
        {
            if (source == null) return;
            var temp = source;
            source = null;
            temp.TrySetResult();
        }
    }
}