using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameBase.ObjectPool
{
    //针对GameObject的复用对象池
    public class MultipleGameObjectPool<K> : CustomDestroyMultipleObjectPool<K, GameObject>
    {
        public MultipleGameObjectPool(int maximumRetained = 10) : base(new PooledGameObjectPolicy(), maximumRetained)
        {
            
        }
    }
}