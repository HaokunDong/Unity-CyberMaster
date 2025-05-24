using System;
using Everlasting.Base;
using UnityEngine;

namespace Everlasting.Extend
{
    public static class ComponentEx
    {
        public static T GetComponentInParent<T>(this Component self, bool includeInactive)
        {
            return (T) (object) GetComponentInParent(self, typeof(T), includeInactive);
        }

        public static Component GetComponentInParent(this Component self, Type type, bool includeInactive)
        {
            if (!includeInactive)
                return self.GetComponentInParent(type);
            Transform parent = self.transform;
            do
            {
                Component c = parent.GetComponent(type);
                if (c != null)
                    return c;
                parent = parent.transform.parent;
                if (parent == null)
                    return null;
            } while (true);
        }
        
        public static TempList<T> GetComponentsInChildrenNonAlloc<T>(this Component self, bool includeInactive)
        {
            var components = new TempList<T>(true);
            self.GetComponentsInChildren(includeInactive, components.List);
            return components;
        }

        public static TempList<T> GetComponentsNonAlloc<T>(this Component self)
        {
            var components = new TempList<T>(true);
            self.GetComponents(components.List);
            return components;
        }
    }
}