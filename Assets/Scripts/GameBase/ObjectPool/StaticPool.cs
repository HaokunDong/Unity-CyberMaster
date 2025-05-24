using System;
using System.Text;
using GameBase;

namespace GameBase.ObjectPool
{
    public static class StaticPool<T> where T : class,new()
    {
        static StaticPool()
        {
            StaticPool<StringBuilder>.InitPolicy(new StringBuilderPooledObjectPolicy());
        }
        
        private static ObjectPool<T> s_objectPool;

        public static T Get()
        {
            s_objectPool ??= ObjectPool.Create<T>();
            return s_objectPool.Get();
        }

        public static void Return(T t)
        {
            s_objectPool ??= ObjectPool.Create<T>();
            s_objectPool.Return(t);
        }

        public static void InitPolicy(IPooledObjectPolicy<T> policy, int maximumRetained = 20, int defaultCount = 0)
        {
            s_objectPool = ObjectPool.Create<T>(policy, maximumRetained, defaultCount);
        }

        public static void SetNoFullWarn()
        {
#if UNITY_EDITOR
            DefaultFastObjectPool<T>.SetNoFullWarn = true;
#endif
        }

        //清空对象池
        public static void ClearPool()
        {
            s_objectPool.Clear();
        }
    }
}