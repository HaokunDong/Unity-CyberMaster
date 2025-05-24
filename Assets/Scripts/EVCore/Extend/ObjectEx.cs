using System.Runtime.CompilerServices;
using UnityEngine;

namespace Everlasting.Extend
{
    public static class ObjectEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Object Instantiate(this Object self)
        {
            return Object.Instantiate(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Object Instantiate(this Object self, Transform parent)
        {
            return Object.Instantiate(self, parent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Object Instantiate(this Object self, GameObject parent)
        {
            return Object.Instantiate(self, parent.transform);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Object Instantiate(this Object self, Transform parent, bool instantiateInWorldSpace)
        {
            return Object.Instantiate(self, parent, instantiateInWorldSpace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Object Instantiate(this Object self, GameObject parent, bool instantiateInWorldSpace)
        {
            return Object.Instantiate(self, parent.transform, instantiateInWorldSpace);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Object Instantiate(this Object self, Vector3 position, Quaternion rotation)
        {
            return Object.Instantiate(self, position, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Object Instantiate(this Object self, Vector3 position, Quaternion rotation, Transform parent)
        {
            return Object.Instantiate(self, position, rotation, parent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Object Instantiate(this Object self, Vector3 position, Quaternion rotation, GameObject parent)
        {
            return Object.Instantiate(self, position, rotation, parent.transform);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Instantiate<T>(this T self) where T : Object
        {
            return Object.Instantiate(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Instantiate<T>(this T self, Vector3 position, Quaternion rotation) where T : Object
        {
            return (T) Object.Instantiate((Object) self, position, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Instantiate<T>(this T self, Vector3 position, Quaternion rotation, Transform parent)
            where T : Object
        {
            return Object.Instantiate(self, position, rotation, parent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Instantiate<T>(this T self, Vector3 position, Quaternion rotation, GameObject parent)
            where T : Object
        {
            return Object.Instantiate(self, position, rotation, parent.transform);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Instantiate<T>(this T self, Transform parent) where T : Object
        {
            return Object.Instantiate<T>(self, parent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Instantiate<T>(this T self, GameObject parent) where T : Object
        {
            return Object.Instantiate<T>(self, parent.transform);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Instantiate<T>(this T self, Transform parent, bool worldPositionStays) where T : Object
        {
            return Object.Instantiate(self, parent, worldPositionStays);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Instantiate<T>(this T self, GameObject parent, bool worldPositionStays) where T : Object
        {
            return Object.Instantiate(self, parent.transform, worldPositionStays);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this Object self)
        {
            Object.Destroy(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this Object self, float delay)
        {
            Object.Destroy(self, delay);
        }
    }
}