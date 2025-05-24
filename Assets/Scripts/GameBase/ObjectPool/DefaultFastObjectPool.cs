using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.ObjectPool
{
    public class DefaultFastObjectPool<T> : ObjectPool<T> where T : class
    {
        private protected Queue<T> _objectQueue = null;
        private protected readonly IPooledObjectPolicy<T> _policy;
        private protected readonly bool _isDefaultPolicy;
        private protected readonly int _maximumRetained;

        public DefaultFastObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained, int defaultCount = 0)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _isDefaultPolicy = IsDefaultPolicy();
            _maximumRetained = maximumRetained;
            _objectQueue = new Queue<T>(maximumRetained);
            defaultCount = Math.Min(maximumRetained, defaultCount);
            for (int i = 0; i < defaultCount; i++)
            {
                Return(Create());
            }
            bool IsDefaultPolicy()
            {
                var type = policy.GetType();

                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DefaultPooledObjectPolicy<>) && !typeof(IReturn).IsAssignableFrom(typeof(T));
            }
        }

        private T Create() => _policy?.Create();

        public override T Get()
        {
            T obj;
            if (_objectQueue.Count > 0)
            {
                obj = _objectQueue.Dequeue();
            }
            else
            {
                obj = Create();
            }
            return obj;
        }

        public override void Return(T obj)
        {
            ReturnCore(obj);
        }

        public override void Clear()
        {
            _objectQueue.Clear();
        }

#if UNITY_EDITOR
        private static bool hasShowPoolFullLog = false;
        public static bool SetNoFullWarn
        {
            set => hasShowPoolFullLog = value;
        }
#endif
        protected bool ReturnCore(T obj)
        {
            bool returnedToPool = false;
            if (_isDefaultPolicy || (_policy.Return(obj)))
            {
                if (_objectQueue.Count <= _maximumRetained)
                {
                    _objectQueue.Enqueue(obj);
                    returnedToPool = true;
                }
#if UNITY_EDITOR
                else if(!hasShowPoolFullLog)
                {
                    hasShowPoolFullLog = true;
                    Type type = typeof(T);
                    Debug.LogWarning($"{type.FullName}对象池已满");
                }
#endif
            }
            return returnedToPool;
        }
    }
}
