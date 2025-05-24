using System;
using System.Threading;

namespace GameBase.ObjectPool
{
    public sealed class DisposableFastObjectPool<T> : DefaultFastObjectPool<T>, IDisposable where T : class
    {
        private bool _isDisposed;

        public DisposableFastObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained, int defaultCount = 0)
            : base(policy, maximumRetained, defaultCount)
        {
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
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}