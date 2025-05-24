using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameBase.ObjectPool
{
    public class PooledGameObjectPolicy : IPooledObjectPolicyWithDestroy<GameObject>
    {
        public GameObject Create()
        {
            return null;
        }

        public bool Return(GameObject obj)
        {
            return true;
        }

        public void Destroy(GameObject obj)
        {
            Object.Destroy(obj);
        }
    }
    
    public class GameObjectPool : CustomDestroyFastObjectPool<GameObject>
    {
        public GameObjectPool(int maximumRetained) : base(new PooledGameObjectPolicy(), maximumRetained, 0)
        {
            
        }
    }
}