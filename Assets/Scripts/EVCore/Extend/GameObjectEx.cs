using System;
using System.Collections.Generic;
using Everlasting.Base;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Everlasting.Extend
{
    public static class GameObjectEx
    {
        /// <summary>
        /// Checks if a GameObject has been destroyed.
        /// </summary>
        /// <param name="gameObject">GameObject reference to check for destructedness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }

        public static void AddChild(this GameObject parent, GameObject child, bool worldPositionStays = true)
        {
            child.transform.SetParent(parent.transform, worldPositionStays);
        }
        public static void AddChild(this GameObject parent, Transform child, bool worldPositionStays = true)
        {
            child.SetParent(parent.transform, worldPositionStays);
        }

        public static GameObject AddNewChild(this GameObject parent, string name = "GameObject")
        {
            var go = new GameObject(name);
            go.SetParent(parent);
            go.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.transform.localScale = Vector3.one;
            return go;
        }

        public static void AddTo(this GameObject child, GameObject parent)
        {
            child.transform.SetParent(parent.transform);
        }

        public static void AddTo(this GameObject child, Transform parent)
        {
            child.transform.SetParent(parent);
        }

        public static GameObject GetParent(this GameObject child)
        {
            var parent = child.transform.parent;
            return parent == null ? null : parent.gameObject;
        }

        public static List<GameObject> GetParents(this GameObject child, bool includeSelf = false)
        {
            List<GameObject> list = new List<GameObject>();
            if (includeSelf)
            {
                list.Add(child);
            }
            var transform = child.transform;
            var parent = transform.parent;
            while (parent != null)
            {
                list.Add(parent.gameObject);
                parent = parent.parent;
            }

            return list;
        }

        public static void SetParent(this GameObject child, GameObject parent)
        {
            child.transform.SetParent(parent.transform);
        }

        public static void SetParent(this GameObject child, Transform parent)
        {
            child.transform.SetParent(parent);
        }

        public static void SetParent(this GameObject child, GameObject parent, bool worldPositionStays)
        {
            child.transform.SetParent(parent.transform, worldPositionStays);
        }

        public static void SetParent(this GameObject child, Transform parent, bool worldPositionStays)
        {
            child.transform.SetParent(parent, worldPositionStays);
        }
        
        public static void SetParentAndReset(this GameObject child, GameObject parent)
        {
            child.transform.SetParent(parent.transform);
            child.ResetPosRotAndScale();
        }

        public static void SetParentAndReset(this GameObject child, Transform parent)
        {
            child.transform.SetParent(parent);
            child.ResetPosRotAndScale();
        }

        public static List<GameObject> GetChildren(this GameObject obj)
        {
            List<GameObject> list = new List<GameObject>();
            obj.GetChildren(list);
            return list;
        }

        public static void GetChildren(this GameObject obj, List<GameObject> list)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                list.Add(obj.transform.GetChild(i).gameObject);
            }
        }

        public static TempList<GameObject> GetChildrenNonAlloc(this GameObject obj)
        {
            var components = new TempList<GameObject>(true);
            obj.GetChildren(components.List);
            return components;
        }

        public static List<GameObject> GetAllChildren(this GameObject obj)
        {
            List<GameObject> list = new List<GameObject>();
            obj.GetAllChildren(list);
            return list;
        }

        public static void GetAllChildren(this GameObject obj, List<GameObject> list)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                var child = obj.transform.GetChild(i).gameObject;
                list.Add(child);
                child.GetAllChildren(list);
            }
        }

        public static void DestroyChildren(this GameObject go)
        {
            List<GameObject> ts = go.GetChildren();
            foreach (var t in ts)
            {
                GameObject.Destroy(t);
            }
        }
        
        public static void DestroyChildrenImmediate(this GameObject go)
        {
            List<GameObject> ts = go.GetChildren();
            foreach (var t in ts)
            {
                GameObject.DestroyImmediate(t);
            }
        }

        public static GameObject GetOrCreateChild(this GameObject go, string name)
        {
            Transform t = go.transform.Find(name);
            if (t)
                return t.gameObject;
            return go.CreateChild(name);
        }
        
        public static GameObject CreateChild(this GameObject go, string name)
        {
            GameObject child = new GameObject(name);
            go.AddChild(child, false);
            return child;
        }

        public static T AddComponentUndo<T>(this GameObject go) where T : Component
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return Undo.AddComponent<T>(go);
            }
#endif
            return go.AddComponent<T>();
        }
        
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T t = go.GetComponent<T>();
            if (t != null)
                return t;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return Undo.AddComponent<T>(go);
            }
#endif
            return go.AddComponent<T>();
        }

        public static Component GetOrAddComponent(this GameObject go, Type type)
        {
            Component t = go.GetComponent(type);
            if (t != null)
                return t;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return Undo.AddComponent(go, type);
            }
#endif
            return go.AddComponent(type);
        }

        public static GameObject FindChild(this GameObject go, string name)
        {
            Transform t = go.transform.Find(name);
            if (t == null)
                return null;
            return t.gameObject;
        }
        
        public static GameObject FindChildRecursion(this GameObject gameObject, string goName)
        {
            var childTransform = gameObject.transform.FindChildRecursion(goName);
            return childTransform ? childTransform.gameObject : null;
        }

        public static void RemoveSelf(this GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go);
        }

        public static void RemoveSelfNextFrame(this GameObject go)
        {
            UnityEngine.Object.Destroy(go, 0.0001f);
        }

        /////////////////////////////////////Transform Op/////////////////////////////////////////

        public static void SetPosition(this GameObject go, float x, float y, float z)
        {
            go.transform.position = new Vector3(x, y, z);
        }

        public static void SetPosition(this GameObject go, Vector3 pos)
        {
            go.transform.position = pos;
        }
        public static Vector3 GetPosition(this GameObject go)
        {
            return go.transform.position;
        }

        public static void SetPositionX(this GameObject go, float x)
        {
            var p = go.transform.position;
            p.x = x;
            go.transform.position = p;
        }

        public static float GetPositionX(this GameObject go)
        {
            return go.transform.position.x;
        }

        public static void SetPositionY(this GameObject go, float y)
        {
            var p = go.transform.position;
            p.y = y;
            go.transform.position = p;
        }

        public static float GetPositionY(this GameObject go)
        {
            return go.transform.position.y;
        }

        public static void SetPositionZ(this GameObject go, float z)
        {
            var p = go.transform.position;
            p.z = z;
            go.transform.position = p;
        }

        public static float GetPositionZ(this GameObject go)
        {
            return go.transform.position.z;
        }

        public static void SetPositionXY(this GameObject go, float x, float y)
        {
            var p = go.transform.position;
            p.x = x;
            p.y = y;
            go.transform.position = p;
        }

        public static void SetPositionXY(this GameObject go, Vector2 xy)
        {
            var p = go.transform.position;
            p.x = xy.x;
            p.y = xy.y;
            go.transform.position = p;
        }

        public static Vector2 GetPositionXY(this GameObject go)
        {
            Vector3 pos = go.transform.position;
            return new Vector2(pos.x, pos.y);
        }

        public static void SetPositionXZ(this GameObject go, float x, float z)
        {
            var p = go.transform.position;
            p.x = x;
            p.z = z;
            go.transform.position = p;
        }

        public static void SetPositionXZ(this GameObject go, Vector2 xz)
        {
            var p = go.transform.position;
            p.x = xz.x;
            p.z = xz.y;
            go.transform.position = p;
        }

        public static Vector2 GetPositionXZ(this GameObject go)
        {
            Vector3 pos = go.transform.position;
            return new Vector2(pos.x, pos.z);
        }

        public static void SetLocalPosition(this GameObject go, float x, float y, float z)
        {
            go.transform.localPosition = new Vector3(x, y, z);
        }

        public static void SetLocalPosition(this GameObject go, Vector3 pos)
        {
            go.transform.localPosition = pos;
        }

        public static Vector3 GetLocalPosition(this GameObject go)
        {
            return go.transform.localPosition;
        }

        public static void SetLocalPositionX(this GameObject go, float x)
        {
            var p = go.transform.localPosition;
            p.x = x;
            go.transform.localPosition = p;
        }

        public static float GetLocalPositionX(this GameObject go)
        {
            return go.transform.localPosition.x;
        }

        public static void SetLocalPositionY(this GameObject go, float y)
        {
            var p = go.transform.localPosition;
            p.y = y;
            go.transform.localPosition = p;
        }

        public static float GetLocalPositionY(this GameObject go)
        {
            return go.transform.localPosition.y;
        }

        public static void SetLocalPositionZ(this GameObject go, float z)
        {
            var p = go.transform.localPosition;
            p.z = z;
            go.transform.localPosition = p;
        }

        public static float GetLocalPositionZ(this GameObject go)
        {
            return go.transform.localPosition.z;
        }

        public static void SetLocalPositionXY(this GameObject go, float x, float y)
        {
            var p = go.transform.localPosition;
            p.x = x;
            p.y = y;
            go.transform.localPosition = p;
        }

        public static void SetLocalPositionXY(this GameObject go, Vector2 xy)
        {
            var p = go.transform.localPosition;
            p.x = xy.x;
            p.y = xy.y;
            go.transform.localPosition = p;
        }

        public static Vector2 GetLocalPositionXY(this GameObject go)
        {
            Vector3 pos = go.transform.localPosition;
            return new Vector2(pos.x, pos.y);
        }

        public static void SetLocalPositionXZ(this GameObject go, float x, float z)
        {
            var p = go.transform.localPosition;
            p.x = x;
            p.z = z;
            go.transform.localPosition = p;
        }

        public static void SetLocalPositionXZ(this GameObject go, Vector2 xz)
        {
            var p = go.transform.localPosition;
            p.x = xz.x;
            p.z = xz.y;
            go.transform.localPosition = p;
        }

        public static Vector2 GetLocalPositionXZ(this GameObject go)
        {
            Vector3 pos = go.transform.localPosition;
            return new Vector2(pos.x, pos.z);
        }

        public static void SetLocalScale(this GameObject go, float scale)
        {
            go.transform.localScale = new Vector3(scale, scale, scale);
        }

        public static void SetLocalScale(this GameObject go, Vector3 scale)
        {
            go.transform.localScale = scale;
        }

        public static void SetLocalScale(this GameObject go, float x, float y, float z)
        {
            go.transform.localScale = new Vector3(x, y, z);
        }

        public static Vector3 GetLocalScale(this GameObject go)
        {
            return go.transform.localScale;
        }

        public static void SetLocalScaleX(this GameObject go, float x)
        {
            var s = go.transform.localScale;
            s.x = x;
            go.transform.localScale = s;
        }
        public static float GetLocalScaleX(this GameObject go)
        {
            return go.transform.localScale.x;
        }

        public static void SetLocalScaleY(this GameObject go, float y)
        {
            var s = go.transform.localScale;
            s.y = y;
            go.transform.localScale = s;
        }
        public static float GetLocalScaleY(this GameObject go)
        {
            return go.transform.localScale.y;
        }

        public static void SetLocalScaleZ(this GameObject go, float z)
        {
            var s = go.transform.localScale;
            s.z = z;
            go.transform.localScale = s;
        }
        public static float GetLocalScaleZ(this GameObject go)
        {
            return go.transform.localScale.z;
        }

        public static void SetLocalScaleXY(this GameObject go, float x, float y)
        {
            var s = go.transform.localScale;
            s.x = x;
            s.y = y;
            go.transform.localScale = s;
        }

        public static void SetLocalScaleXY(this GameObject go, Vector2 xy)
        {
            var s = go.transform.localScale;
            s.x = xy.x;
            s.y = xy.y;
            go.transform.localScale = s;
        }

        public static Vector2 GetLocalScaleXY(this GameObject go)
        {
            var s = go.transform.localScale;
            return new Vector2(s.x, s.y);
        }

        public static void SetLocalScaleXZ(this GameObject go, float x, float z)
        {
            var s = go.transform.localScale;
            s.x = x;
            s.z = z;
            go.transform.localScale = s;
        }

        public static void SetLocalScaleXZ(this GameObject go, Vector2 xz)
        {
            var s = go.transform.localScale;
            s.x = xz.x;
            s.z = xz.y;
            go.transform.localScale = s;
        }

        public static Vector2 GetLocalScaleXZ(this GameObject go)
        {
            var s = go.transform.localScale;
            return new Vector2(s.x, s.z);
        }
        
        public static void SetLocalRotation(this GameObject go, Quaternion rotation)
        {
            go.transform.localRotation = rotation;
        }

        public static Quaternion GetLocalRotation(this GameObject go)
        {
            return go.transform.localRotation;
        }

        public static Vector3 GetEulerAngles(this GameObject go)
        {
            return go.transform.eulerAngles;
        }
        
        public static void SetEulerAngles(this GameObject go, Vector3 eulerAngle)
        {
            go.transform.eulerAngles = eulerAngle;
        }
        
        public static void SetEulerAngles(this GameObject go, float x, float y, float z)
        {
            go.transform.eulerAngles = new Vector3(x, y, z);
        }
        
        public static float GetEulerAnglesX(this GameObject go)
        {
            return go.transform.eulerAngles.x;
        }
        
        public static void SetEulerAnglesX(this GameObject go, float x)
        {
            var temp = go.transform.eulerAngles;
            temp.x = x;
            go.transform.eulerAngles = temp;
        }
        
        public static float GetEulerAnglesY(this GameObject go)
        {
            return go.transform.eulerAngles.y;
        }
        
        public static void SetEulerAnglesY(this GameObject go, float y)
        {
            var temp = go.transform.eulerAngles;
            temp.y = y;
            go.transform.eulerAngles = temp;
        }
        
        public static float GetEulerAnglesZ(this GameObject go)
        {
            return go.transform.eulerAngles.z;
        }
        
        public static void SetEulerAnglesZ(this GameObject go, float z)
        {
            var temp = go.transform.eulerAngles;
            temp.z = z;
            go.transform.eulerAngles = temp;
        }
        
        public static void SetLocalEulerAngles(this GameObject go, float x, float y, float z)
        {
            go.transform.localEulerAngles = new Vector3(x, y, z);
        }

        public static void SetLocalEulerAngles(this GameObject go, Vector3 eulerAngle)
        {
            go.transform.localEulerAngles = eulerAngle;
        }

        public static Vector3 GetLocalEulerAngles(this GameObject go)
        {
            return go.transform.localEulerAngles;
        }

        public static void SetLocalEulerAnglesX(this GameObject go, float x)
        {
            var e = go.transform.localEulerAngles;
            e.x = x;
            go.transform.localEulerAngles = e;
        }

        public static float GetLocalEulerAnglesX(this GameObject go)
        {
            return go.transform.localEulerAngles.x;
        }

        public static void SetLocalEulerAnglesY(this GameObject go, float y)
        {
            var e = go.transform.localEulerAngles;
            e.y = y;
            go.transform.localEulerAngles = e;
        }

        public static float GetLocalEulerAnglesY(this GameObject go)
        {
            return go.transform.localEulerAngles.y;
        }

        public static void SetLocalEulerAnglesZ(this GameObject go, float z)
        {
            var e = go.transform.localEulerAngles;
            e.z = z;
            go.transform.localEulerAngles = e;
        }

        public static float GetLocalEulerAnglesZ(this GameObject go)
        {
            return go.transform.localEulerAngles.z;
        }


        public static Vector3 InverseTransformDirection(this GameObject go, Vector3 direction)
        {
            return go.transform.InverseTransformDirection(direction);
        }

        public static Vector3 InverseTransformDirection(this GameObject go, float x, float y, float z)
        {
            return go.transform.InverseTransformDirection(x, y, z);
        }

        public static Vector3 InverseTransformPoint(this GameObject go, Vector3 position)
        {
            return go.transform.InverseTransformPoint(position);
        }

        public static Vector3 InverseTransformPoint(this GameObject go, float x, float y, float z)
        {
            return go.transform.InverseTransformPoint(x, y, z);
        }

        public static Vector3 InverseTransformVector(this GameObject go, Vector3 vector)
        {
            return go.transform.InverseTransformVector(vector);
        }

        public static Vector3 InverseTransformVector(this GameObject go, float x, float y, float z)
        {
            return go.transform.InverseTransformVector(x, y, z);
        }

        public static void SetPositionAndRotation(this GameObject go, Vector3 position, Quaternion rotation)
        {
            go.transform.SetPositionAndRotation(position, rotation);
        }
        
        public static void ResetPosRotAndScale(this GameObject go)
        {
            go.transform.ResetPosRotAndScale();
        }

        public static Vector3 TransformDirection(this GameObject go, Vector3 direction)
        {
            return go.transform.TransformDirection(direction);
        }

        public static Vector3 TransformDirection(this GameObject go, float x, float y, float z)
        {
            return go.transform.TransformDirection(x, y, z);
        }

        public static Vector3 TransformPoint(this GameObject go, Vector3 position)
        {
            return go.transform.TransformPoint(position);
        }

        public static Vector3 TransformPoint(this GameObject go, float x, float y, float z)
        {
            return go.transform.TransformPoint(x, y, z);
        }

        public static Vector3 TransformVector(this GameObject go, Vector3 vector)
        {
            return go.transform.TransformVector(vector);
        }

        public static Vector3 TransformVector(this GameObject go, float x, float y, float z)
        {
            return go.transform.TransformVector(x, y, z);
        }

        public static Vector3 GetForward(this GameObject go)
        {
            return go.transform.forward;
        }

        public static void Rotate(this GameObject go, Vector3 axis, float angle)
        {
            go.transform.Rotate(axis, angle);
        }

        public static void RotateAround(this GameObject go, Vector3 point, Vector3 axis, float angle)
        {
            go.transform.RotateAround(point, axis, angle);
        }

        /// <summary>
        ///  给一个组件访问, 没有GC的
        ///  该方法得到的数据，只能临时引用，不能持久保存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="includeInactive"></param>
        /// <returns></returns>
        public static TempList<T> GetComponentsInChildrenNonAlloc<T>(this GameObject go, bool includeInactive)
        {
            var components = new TempList<T>(true);
            go.GetComponentsInChildren(includeInactive, components.List);
            return components;
        }

        public static TempList<T> GetComponentsNonAlloc<T>(this GameObject go)
        {
            var components = new TempList<T>(true);
            go.GetComponents(components.List);
            return components;
        }
        
        public static Transform GetChild(this GameObject go, int index)
        {
            return go.transform.GetChild(index);
        }

        public static bool IsChildOf(this GameObject go, GameObject target, bool recursive = false)
        {
            return go.transform.IsChildOf(target.transform, recursive);
        }

        public static string GetPath(this GameObject go)
        {
            return go.transform.GetPath();
        }
        
#if !UNITY_2020_OR_NEWER
        public static T GetComponentInParent<T>(this GameObject self, bool includeInactive)
        {
            return (T)(object) GetComponentInParent(self, typeof(T), includeInactive);
        }

        public static Component GetComponentInParent(this GameObject self, Type type, bool includeInactive)
        {
            if (!includeInactive)
                return self.GetComponentInParent(type);
            Transform parent = self.transform;
            do
            {
                Component c = parent.GetComponent(type);
                if (c != null)
                    return c;
                parent = parent.parent;
                if (parent == null)
                    return null;
            } while (true);
        }
#endif
        
        ////////////////////////// GetComponentInParent 支持多Component作为参数 扩展 /////////////////////
        public static Component GetComponentInParent(this GameObject self, params Type[] ts)
        {
            return self.transform.GetComponentInParent(ts);
        }

        public static Tuple<T1, T2> GetComponentInParent<T1, T2>(this GameObject self) where T1 : Component where T2 : Component
        {
            return self.transform.GetComponentInParent<T1, T2>();
        }

        public static Tuple<T1, T2, T3> GetComponentInParent<T1, T2, T3>(this GameObject self) where T1 : Component where T2 : Component where T3 : Component
        {
            return self.transform.GetComponentInParent<T1, T2, T3>();
        }

        public static bool IsPartOfCurrentPrefabStage(this GameObject self)
        {
#if UNITY_EDITOR
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            return prefabStage != null && prefabStage.IsPartOfPrefabContents(self);
#else
            return false;
#endif
        }

        public static GameObject GetNearestPrefabRoot(this GameObject self)
        {
#if UNITY_EDITOR
            return PrefabUtility.GetNearestPrefabInstanceRoot(self);
#else
            return null;
#endif
        }
        
        public static GameObject GetOutermostPrefabRoot(this GameObject self)
        {
#if UNITY_EDITOR
            return PrefabUtility.GetOutermostPrefabInstanceRoot(self);
#else
            return null;
#endif
        }

        public static GameObject FindParent(this GameObject self, Func<GameObject, bool> check)
        {
            if(self == null) return null;
            if (check(self)) return self;
            return FindParent(self.GetParent(), check);
        }
    }
}
