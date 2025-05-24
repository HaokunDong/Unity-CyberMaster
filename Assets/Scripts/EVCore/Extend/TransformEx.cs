using System;
using System.Collections.Generic;
using Everlasting.Base;
using UnityEngine;

namespace Everlasting.Extend
{
    public static class TransformEx
    {
        public static bool IsDestroyed(this Transform transform)
        {
            return transform.gameObject.IsDestroyed();
        }

        public static void AddChild(this Transform parent, GameObject child)
        {
            child.transform.SetParent(parent);
        }

        public static void AddChild(this Transform parent, Transform child)
        {
            child.SetParent(parent);
        }

        public static void AddTo(this Transform child, GameObject parent)
        {
            child.SetParent(parent.transform);
        }

        public static void AddTo(this Transform child, Transform parent)
        {
            child.SetParent(parent);
        }

        public static Transform GetParent(this Transform child)
        {
            if (child.parent == null)
                return null;
            return child.parent;
        }

        public static void SetParent(this Transform child, GameObject parent)
        {
            child.SetParent(parent.transform);
        }
        
        public static void SetParentAndReset(this Transform child, Transform parent)
        {
            child.SetParent(parent);
            child.ResetPosRotAndScale();
        }
        
        public static void SetParentAndReset(this Transform child, GameObject parent)
        {
            child.SetParent(parent.transform);
            child.ResetPosRotAndScale();
        }
        
        public static void ResetPosRotAndScale(this Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }

        public static List<Transform> GetChildren(this Transform transform)
        {
            List<Transform> list = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                list.Add(transform.GetChild(i));
            }
            return list;
        }

        public static TempList<Transform> GetChildrenNonAlloc(this Transform transform)
        {
            var components = new TempList<Transform>(true);
            transform.GetChildren(components.List);
            return components;
        }

        public static void GetChildren(this Transform transform, List<Transform> list)
        {
            list.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                list.Add(transform.GetChild(i));
            }
        }

        public static void ClearChildren(this Transform transform)
        {
            List<Transform> ts = transform.GetChildren();
            foreach (var t in ts)
            {
                UnityEngine.Object.Destroy(t);
            }
        }

        public static Transform GetOrCreateChild(this Transform transform, string name)
        {
            Transform t = transform.Find(name);
            if (t)
                return t;
            GameObject child = new GameObject(name);
            transform.AddChild(child);
            return child.transform;
        }
        
        public static Transform FindChildRecursion(this Transform transform, string goName)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name.Equals(goName)) return child;
                child = FindChildRecursion(child, goName);
                if (child != null)
                    return child;
            }
            return null;
        }

        public static T GetOrAddComponent<T>(this Transform transform) where T : Component
        {
            T t = transform.GetComponent<T>();
            if (t != null)
                return t;
            return transform.gameObject.AddComponent<T>();
        }

        public static Component GetOrAddComponent(this Transform transform, Type type)
        {
            Component t = transform.GetComponent(type);
            if (t != null)
                return t;
            return transform.gameObject.AddComponent(type);
        }

        public static void RemoveSelf(this Transform transform)
        {
            UnityEngine.Object.DestroyImmediate(transform.gameObject);
        }

        public static void RemoveSelfNextFrame(this Transform transform)
        {
            UnityEngine.Object.Destroy(transform.gameObject, 0.0001f);
        }

        /////////////////////////////////////Transform Op/////////////////////////////////////////
        public static void CopyTransform(this Transform transform, Transform target)
        {
            transform.position = target.position;
            transform.eulerAngles = target.eulerAngles;
        }

        public static void SetPosition(this Transform transform, float x, float y, float z)
        {
            transform.position = new Vector3(x, y, z);
        }

        public static void SetPosition(this Transform transform, Vector3 pos)
        {
            transform.position = pos;
        }
        public static Vector3 GetPosition(this Transform transform)
        {
            return transform.position;
        }

        public static void SetPositionX(this Transform transform, float x)
        {
            var p = transform.position;
            p.x = x;
            transform.position = p;
        }

        public static float GetPositionX(this Transform transform)
        {
            return transform.position.x;
        }

        public static void SetPositionY(this Transform transform, float y)
        {
            var p = transform.position;
            p.y = y;
            transform.position = p;
        }

        public static float GetPositionY(this Transform transform)
        {
            return transform.position.y;
        }

        public static void SetPositionZ(this Transform transform, float z)
        {
            var p = transform.position;
            p.z = z;
            transform.position = p;
        }

        public static float GetPositionZ(this Transform transform)
        {
            return transform.position.z;
        }

        public static void SetPositionXY(this Transform transform, float x, float y)
        {
            var p = transform.position;
            p.x = x;
            p.y = y;
            transform.position = p;
        }

        public static void SetPositionXY(this Transform transform, Vector2 xy)
        {
            var p = transform.position;
            p.x = xy.x;
            p.y = xy.y;
            transform.position = p;
        }

        public static Vector2 GetPositionXY(this Transform transform)
        {
            Vector3 pos = transform.position;
            return new Vector2(pos.x, pos.y);
        }

        public static void SetPositionXZ(this Transform transform, float x, float z)
        {
            var p = transform.position;
            p.x = x;
            p.z = z;
            transform.position = p;
        }

        public static void SetPositionXZ(this Transform transform, Vector2 xz)
        {
            var p = transform.position;
            p.x = xz.x;
            p.z = xz.y;
            transform.position = p;
        }

        public static Vector2 GetPositionXZ(this Transform transform)
        {
            Vector3 pos = transform.position;
            return new Vector2(pos.x, pos.z);
        }


        public static void SetLocalPosition(this Transform transform, float x, float y, float z)
        {
            transform.localPosition = new Vector3(x, y, z);
        }

        public static void SetLocalPosition(this Transform transform, Vector3 pos)
        {
            transform.localPosition = pos;
        }

        public static Vector3 GetLocalPosition(this Transform transform)
        {
            return transform.localPosition;
        }

        public static void SetLocalPositionX(this Transform transform, float x)
        {
            var p = transform.localPosition;
            p.x = x;
            transform.localPosition = p;
        }

        public static float GetLocalPositionX(this Transform transform)
        {
            return transform.localPosition.x;
        }

        public static void SetLocalPositionY(this Transform transform, float y)
        {
            var p = transform.localPosition;
            p.y = y;
            transform.localPosition = p;
        }

        public static float GetLocalPositionY(this Transform transform)
        {
            return transform.localPosition.y;
        }

        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            var p = transform.localPosition;
            p.z = z;
            transform.localPosition = p;
        }

        public static float GetLocalPositionZ(this Transform transform)
        {
            return transform.localPosition.z;
        }

        public static void SetLocalPositionXY(this Transform transform, float x, float y)
        {
            var p = transform.localPosition;
            p.x = x;
            p.y = y;
            transform.localPosition = p;
        }

        public static void SetLocalPositionXY(this Transform transform, Vector2 xy)
        {
            var p = transform.localPosition;
            p.x = xy.x;
            p.y = xy.y;
            transform.localPosition = p;
        }

        public static Vector2 GetLocalPositionXY(this Transform transform)
        {
            Vector3 pos = transform.localPosition;
            return new Vector2(pos.x, pos.y);
        }
        public static void GetLocalPositionXYZ(this Transform transform,ref float x,ref float y,ref float z)
        {
            Vector3 pos = transform.localPosition;
            x = pos.x;
            y = pos.y;
            z = pos.z;
        }
        public static void GetPositionXYZ(this Transform transform, ref float x, ref float y, ref float z)
        {
            Vector3 pos = transform.position;
            x = pos.x;
            y = pos.y;
            z = pos.z;
        }
        public static void SetLocalPositionXZ(this Transform transform, float x, float z)
        {
            var p = transform.localPosition;
            p.x = x;
            p.z = z;
            transform.localPosition = p;
        }

        public static void SetLocalPositionXZ(this Transform transform, Vector2 xz)
        {
            var p = transform.localPosition;
            p.x = xz.x;
            p.z = xz.y;
            transform.localPosition = p;
        }
        public static void SetOffsetLocalPositionXZ(this Transform transform, float x, float z)
        {
            var p = transform.localPosition;
            p.x += x;
            p.z += z;
            transform.localPosition = p;
        }
        public static Vector2 GetLocalPositionXZ(this Transform transform)
        {
            Vector3 pos = transform.localPosition;
            return new Vector2(pos.x, pos.z);
        }

        public static void SetLocalScale(this Transform transform, float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }

        public static void SetLocalScale(this Transform transform, Vector3 scale)
        {
            transform.localScale = scale;
        }

        public static void SetLocalScale(this Transform transform, float x, float y, float z)
        {
            transform.localScale = new Vector3(x, y, z);
        }

        public static Vector3 GetLocalScale(this Transform transform)
        {
            return transform.localScale;
        }

        public static void SetLocalScaleX(this Transform transform, float x)
        {
            var s = transform.localScale;
            s.x = x;
            transform.localScale = s;
        }
        public static float GetLocalScaleX(this Transform transform)
        {
            return transform.localScale.x;
        }

        public static void SetLocalScaleY(this Transform transform, float y)
        {
            var s = transform.localScale;
            s.y = y;
            transform.localScale = s;
        }
        public static float GetLocalScaleY(this Transform transform)
        {
            return transform.localScale.y;
        }

        public static void SetLocalScaleZ(this Transform transform, float z)
        {
            var s = transform.localScale;
            s.z = z;
            transform.localScale = s;
        }
        public static float GetLocalScaleZ(this Transform transform)
        {
            return transform.localScale.z;
        }

        public static void SetLocalScaleXY(this Transform transform, float x, float y)
        {
            var s = transform.localScale;
            s.x = x;
            s.y = y;
            transform.localScale = s;
        }

        public static void SetLocalScaleXY(this Transform transform, Vector2 xy)
        {
            var s = transform.localScale;
            s.x = xy.x;
            s.y = xy.y;
            transform.localScale = s;
        }

        public static Vector2 GetLocalScaleXY(this Transform transform)
        {
            var s = transform.localScale;
            return new Vector2(s.x, s.y);
        }

        public static void SetLocalScaleXZ(this Transform transform, float x, float z)
        {
            var s = transform.localScale;
            s.x = x;
            s.z = z;
            transform.localScale = s;
        }

        public static void SetLocalScaleXZ(this Transform transform, Vector2 xz)
        {
            var s = transform.localScale;
            s.x = xz.x;
            s.z = xz.y;
            transform.localScale = s;
        }

        public static Vector2 GetLocalScaleXZ(this Transform transform)
        {
            var s = transform.localScale;
            return new Vector2(s.x, s.z);
        }

        public static void SetLocalRotation(this Transform transform, Quaternion rotation)
        {
            transform.localRotation = rotation;
        }

        public static Quaternion GetLocalRotation(this Transform transform)
        {
            return transform.localRotation;
        }

        public static void SetLocalEulerAngles(this Transform transform, float x, float y, float z)
        {
            transform.localEulerAngles = new Vector3(x, y, z);
        }

        public static void SetLocalEulerAngles(this Transform transform, Vector3 eulerAngle)
        {
            transform.localEulerAngles = eulerAngle;
        }

        public static Vector3 GetLocalEulerAngles(this Transform transform)
        {
            return transform.localEulerAngles;
        }

        public static void SetLocalEulerAnglesX(this Transform transform, float x)
        {
            var e = transform.localEulerAngles;
            e.x = x;
            transform.localEulerAngles = e;
        }

        public static float GetLocalEulerAnglesX(this Transform transform)
        {
            return transform.localEulerAngles.x;
        }

        public static void SetLocalEulerAnglesY(this Transform transform, float y)
        {
            var e = transform.localEulerAngles;
            e.y = y;
            transform.localEulerAngles = e;
        }

        public static float GetLocalEulerAnglesY(this Transform transform)
        {
            return transform.localEulerAngles.y;
        }

        public static void SetLocalEulerAnglesZ(this Transform transform, float z)
        {
            var e = transform.localEulerAngles;
            e.z = z;
            transform.localEulerAngles = e;
        }

        public static float GetLocalEulerAnglesZ(this Transform transform)
        {
            return transform.localEulerAngles.z;
        }

        public static Vector3 GetForward(this Transform transform)
        {
            return transform.forward;
        }
        public static void RotateY(this Transform transform,float y)
        {
            transform.Rotate(0, y, 0);
        }

        public static Bounds TransformBounds(this Transform self, Bounds bounds)
        {
            var center = self.TransformPoint(bounds.center);
            var points = bounds.GetCorners();

            var result = new Bounds(center, Vector3.zero);
            foreach (var point in points)
                result.Encapsulate(self.TransformPoint(point));
            return result;
        }
        public static Bounds InverseTransformBounds(this Transform self, Bounds bounds)
        {
            var center = self.InverseTransformPoint(bounds.center);
            var points = bounds.GetCorners();

            var result = new Bounds(center, Vector3.zero);
            foreach (var point in points)
                result.Encapsulate(self.InverseTransformPoint(point));
            return result;
        }

        public static bool IsChildOf(this Transform self, Transform target, bool recursive = false)
        {
            foreach (Transform child in target)
            {
                if (child == self)
                    return true;
                if (recursive)
                {
                    if (self.IsChildOf(child, true))
                        return true;
                }
            }
            return false;
        }
        public static void LookAt(this Transform self,float x,float y,float z)
        {
            self.LookAt(new Vector3(x, y, z));
        }

        public static string GetPath(this Transform self)
        {
            using (var names = new TempList<string>(true))
            {
                Transform trans = self;
                do
                {
                    names.List.Add(trans.gameObject.name);
                    trans = trans.parent;
                } while (trans);

                names.List.Reverse();

                return string.Join("/", names.List);
            }
        }
        
        //找到从某节点开始的路径
        public static string GetPathUntilParent(this Transform self, Transform parent, bool containParentSelf = true)
        {
            using (var names = new TempList<string>(true))
            {
                Transform trans = self;
                do
                {
                    if (trans == parent)
                    {
                        if(containParentSelf) names.List.Add(trans.gameObject.name);
                        break;
                    }
                    names.List.Add(trans.gameObject.name);
                    trans = trans.parent;
                } while (trans);
                
                names.List.Reverse();

                return string.Join("/", names.List);
            }
        }
        
        ////////////////////////// GetComponentInParent 支持多Component作为参数 扩展 /////////////////////
        public static Component GetComponentInParent(this Transform self, params Type[] ts)
        {
            Transform trans = self;
            while (trans != null)
            {
                foreach (var t in ts)
                {
                    Component c = trans.GetComponent(t);
                    if (c != null)
                        return c;
                }
                trans = trans.parent;
            }
            return null;
        }

        public static Tuple<T1, T2> GetComponentInParent<T1, T2>(this Transform self) where T1 : Component where T2 : Component
        {
            Transform trans = self;
            while (trans != null)
            {
                var t1 = trans.GetComponent<T1>();
                if (t1 != null)
                    return new Tuple<T1, T2>(t1, null);
                var t2 = trans.GetComponent<T2>();
                if (t2 != null)
                    return new Tuple<T1, T2>(null, t2);
                trans = trans.parent;
            }
            return new Tuple<T1, T2>(null, null);
        }

        public static Tuple<T1, T2, T3> GetComponentInParent<T1, T2, T3>(this Transform self) where T1 : Component where T2 : Component where T3 : Component
        {
            Transform trans = self;
            while (trans != null)
            {
                var t1 = trans.GetComponent<T1>();
                if (t1 != null)
                    return new Tuple<T1, T2, T3>(t1, null, null);
                var t2 = trans.GetComponent<T2>();
                if (t2 != null)
                    return new Tuple<T1, T2, T3>(null, t2, null);
                var t3 = trans.GetComponent<T3>();
                if (t3 != null)
                    return new Tuple<T1, T2, T3>(null, null, t3);
                trans = trans.parent;
            }
            return new Tuple<T1, T2, T3>(null, null, null);
        }

        public static void ForEachChild(this Transform self, Action<Transform> callback)
        {
            for (int i = 0; i < self.childCount; i++)
            {
                var child = self.GetChild(i);
                callback(child);
            }
        }
        
        public static void ForEachChildRecursion(this Transform self, Action<Transform> callback)
        {
            for (int i = 0; i < self.childCount; i++)
            {
                var child = self.GetChild(i);
                callback(child);
                ForEachChildRecursion(child, callback);
            }
        }
        
        public static Transform FindParent(this Transform self, Func<Transform, bool> check)
        {
            if(self == null) return null;
            if (check(self)) return self;
            return FindParent(self.parent, check);
        }
    }
}
