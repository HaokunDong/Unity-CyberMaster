using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace AssetLoad
{
    public class InstantiateObj : MonoBehaviour
    {
        public int originalInstanceID;
        
        private void Awake()
        {
            int instanceID = gameObject.GetInstanceID();
            if (originalInstanceID != instanceID)
            {
                ResourceManager.AddAssetRef(originalInstanceID, instanceID);
            }
        }

        private void OnDestroy()
        {
            ResourceManager.DelAssetRef(gameObject.GetInstanceID());
        }
    }
}
