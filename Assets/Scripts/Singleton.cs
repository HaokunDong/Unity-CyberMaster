using System;
using System.Threading;

public class Singleton<T> where T : new()
{
    private static T s_Instance = default(T);
    private static object s_objectLock = new object();
    public static T Ins
    {
        get
        {
            if (s_Instance == null)
            {
                object obj;
                Monitor.Enter(obj = s_objectLock);//加锁防止多线程创建单例
                try
                {
                    if (s_Instance == null)
                    {
                        s_Instance = ((default(T) == null) ? Activator.CreateInstance<T>() : default(T));//创建单例的实例
                    }
                }
                finally
                {
                    Monitor.Exit(obj);
                }
            }
            return s_Instance;
        }
    }

    protected Singleton()
    {

    }
}