using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public class BaseObjectPool
    {
        private Dictionary<string, BaseObject> m_baseObjectDict = null;

        public BaseObjectPool()
        {
            m_baseObjectDict = new Dictionary<string, BaseObject>();
        }

        public Dictionary<string, BaseObject> GetBaseObjectPool()
        {
            return m_baseObjectDict;
        }

        public ObjectT AddBaseObject<ObjectT>() where ObjectT : BaseObject, new()
        {
            ObjectT objT = null;
            if (m_baseObjectDict != null)
            {
                Type objType = typeof(ObjectT);
                string typeName = objType.FullName;
                objT = new ObjectT();
                #if UNITY_EDITOR
                if (m_baseObjectDict.ContainsKey(typeName))
                {
                    Debug.LogError("duplicate create : " + typeName);
                }
                #endif
                m_baseObjectDict[typeName] = objT;
            }
            return objT;
        }

        public ObjectT GetBaseObject<ObjectT>() where ObjectT : BaseObject, new()
        {
            if (m_baseObjectDict != null)
            {
                Type objType = typeof(ObjectT);
                string typeName = objType.FullName;

                BaseObject ret = null;
                m_baseObjectDict.TryGetValue(typeName, out ret);
                if (ret != null)
                {
                    return ret as ObjectT;
                }
            }
            return null;
        }
    }
}
