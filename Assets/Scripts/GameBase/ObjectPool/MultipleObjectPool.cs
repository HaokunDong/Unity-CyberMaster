using System;
using System.Collections.Generic;

namespace GameBase.ObjectPool
{
    public abstract class BaseMultipleObjectPool<K, V, TPool> where V : class where TPool : ObjectPool<V>
    {
        protected Dictionary<K, TPool> m_objectPools = new Dictionary<K, TPool>();
        private IPooledObjectPolicy<V> m_policy = null;
        private int m_maximumRetained;

        public BaseMultipleObjectPool(IPooledObjectPolicy<V> policy, int maximumRetained = 10)
        {
            m_policy = policy;
            m_maximumRetained = maximumRetained;
        }

        protected abstract TPool CreateNewPool(IPooledObjectPolicy<V> policy, int maximumRetained);
        
        private TPool GetOrAddObjectPool(K key)
        {
            TPool pool;
            if (!m_objectPools.TryGetValue(key, out pool))
            {
                pool = CreateNewPool(m_policy, m_maximumRetained);
                m_objectPools.Add(key, pool);
            }

            return pool;
        }

        public virtual void Remove(K key)
        {
            m_objectPools.Remove(key);
        }
        
        public V Get(K key)
        {
            return GetOrAddObjectPool(key).Get();
        }
        
        public void Return(K key, V obj)
        {
            GetOrAddObjectPool(key).Return(obj);
        }
    }
    
    public class MultipleObjectPool<K, V> : BaseMultipleObjectPool<K, V, DefaultFastObjectPool<V>> where V : class
    {
        public MultipleObjectPool(PooledObjectPolicy<V> policy, int maximumRetained = 10) : base(policy, maximumRetained) { }
        
        protected override DefaultFastObjectPool<V> CreateNewPool(IPooledObjectPolicy<V> policy, int maximumRetained)
        {
            return new DefaultFastObjectPool<V>(policy, maximumRetained);
        }
    }

    public class DisposableMultipleObjectPool<K, V> : BaseMultipleObjectPool<K, V, DisposableFastObjectPool<V>>, IDisposable where V : class, IDisposable
    {
        public DisposableMultipleObjectPool(IPooledObjectPolicy<V> policy, int maximumRetained = 10) : base(policy, maximumRetained){}
        protected override DisposableFastObjectPool<V> CreateNewPool(IPooledObjectPolicy<V> policy, int maximumRetained)
        {
            return new DisposableFastObjectPool<V>(policy, maximumRetained);
        }

        public override void Remove(K key)
        {
            if (m_objectPools.TryGetValue(key, out var pool))
            {
                pool.Dispose();
                m_objectPools.Remove(key);
            }
        }

        public void Clear()
        {
            foreach (var pool in m_objectPools.Values)
            {
                pool.Dispose();
            }
            m_objectPools.Clear();
        }
        
        public void Dispose()
        {
            Clear();
        }
    }
    
    public class CustomDestroyMultipleObjectPool<K, V> : BaseMultipleObjectPool<K, V, CustomDestroyFastObjectPool<V>>, IDisposable where V : class
    {
        public CustomDestroyMultipleObjectPool(IPooledObjectPolicyWithDestroy<V> policy, int maximumRetained = 10) : base(policy, maximumRetained){}
        
        protected override CustomDestroyFastObjectPool<V> CreateNewPool(IPooledObjectPolicy<V> policy, int maximumRetained)
        {
            return new CustomDestroyFastObjectPool<V>(policy as IPooledObjectPolicyWithDestroy<V>, maximumRetained);
        }

        public override void Remove(K key)
        {
            if (m_objectPools.TryGetValue(key, out var pool))
            {
                pool.Dispose();
                m_objectPools.Remove(key);
            }
        }

        public void Clear()
        {
            foreach (var pool in m_objectPools.Values)
            {
                pool.Dispose();
            }
            m_objectPools.Clear();
        }
        
        public void Dispose()
        {
            Clear();
        }
    }
}