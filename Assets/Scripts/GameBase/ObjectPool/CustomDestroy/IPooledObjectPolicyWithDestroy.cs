namespace GameBase.ObjectPool
{
    public interface IPooledObjectPolicyWithDestroy<T> : IPooledObjectPolicy<T> where T : notnull
    {
        void Destroy(T obj);
    }
}