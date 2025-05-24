using System;

namespace GameBase.ObjectPool
{
    //根据Microsoft官方的对象池作了一些修改，增加了IReturn以及StaticPool
    /// <summary>
    /// A pool of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to pool.</typeparam>
    public abstract class ObjectPool<T>
    {
        /// <summary>
        /// Gets an object from the pool if one is available, otherwise creates one.
        /// </summary>
        /// <returns>A <typeparamref name="T"/>.</returns>
        public abstract T Get();

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        /// <param name="obj">The object to add to the pool.</param>
        public abstract void Return(T obj);

        public abstract void Clear();
    }

    /// <summary>
    /// Methods for creating <see cref="ObjectPool{T}"/> instances.
    /// </summary>
    public static class ObjectPool
    {
        /// <inheritdoc />
        public static ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy = null, int maximumRetained = 20, int defaultCount = 0) where T : class, new()
        {
            policy ??= new DefaultPooledObjectPolicy<T>();
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            {
                return new DisposableFastObjectPool<T>(policy, maximumRetained, defaultCount);
            }
            return new DefaultFastObjectPool<T>(policy, maximumRetained, defaultCount);
        }
    }
}