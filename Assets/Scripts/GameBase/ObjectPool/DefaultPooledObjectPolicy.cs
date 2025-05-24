namespace GameBase.ObjectPool
{
    public class DefaultPooledObjectPolicy<T> : PooledObjectPolicy<T> where T : class, new()
    {
        public override T Create()
        {
            return new T();
        }

        // DefaultObjectPool<T> doesn't call 'Return' for the default policy.
        // So take care adding any logic to this method, as it might require changes elsewhere.
        public override bool Return(T obj)
        {
            ReturnItem(obj);
            return true;
        }
        
        private void ReturnItem(T obj)
        {
            if (obj is IReturn iReturn)
            {
                iReturn.OnReturn();
            }
        }
    }
}