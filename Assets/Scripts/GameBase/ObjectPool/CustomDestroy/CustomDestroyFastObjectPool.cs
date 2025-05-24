using System;

namespace GameBase.ObjectPool
{
    public class CustomDestroyFastObjectPool<T> : DefaultFastObjectPool<T>, IDisposable where T : class
    {
        private bool _isDisposed;
        private IPooledObjectPolicyWithDestroy<T> m_policyNew;

        public CustomDestroyFastObjectPool(IPooledObjectPolicyWithDestroy<T> policy, int maximumRetained, int defaultCount = 0)
            : base(policy, maximumRetained, defaultCount)
        {
            m_policyNew = policy;
        }

        public override T Get()
        {
            if (_isDisposed)
            {
                ThrowObjectDisposedException();
            }

            return base.Get();

            void ThrowObjectDisposedException()
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public override void Return(T obj)
        {
            // When the pool is disposed or the obj is not returned to the pool, dispose it
            if (_isDisposed || !ReturnCore(obj))
            {
                DisposeItem(obj);
            }
        }

        public void Dispose()
        {
            _isDisposed = true;
            int count = _objectQueue.Count;
            for (int i = 0; i < count; i++)
            {
                DisposeItem(_objectQueue.Dequeue());
            }
            _objectQueue.Clear();
        }

        private void DisposeItem(T item)
        {
            m_policyNew.Destroy(item);
        }
    }
}