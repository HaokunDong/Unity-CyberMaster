namespace GameBase.ObjectPool
{
    //不会创建对应的可以用于Gameobject、Monobehavior等
    public class NonCreatePooledObjectPolicy<T> : PooledObjectPolicy<T> where T : class
    {
        private bool useInterfaceReturn = false;
        
        public NonCreatePooledObjectPolicy()
        {
            useInterfaceReturn = typeof(IReturn).IsAssignableFrom(typeof(T));
        }
        
        public override T Create()
        {
            return null;
        }

        // DefaultObjectPool<T> doesn't call 'Return' for the default policy.
        // So take care adding any logic to this method, as it might require changes elsewhere.
        public override bool Return(T obj)
        {
            if(useInterfaceReturn) ReturnItem(obj);
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