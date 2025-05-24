using UnityEngine;

namespace GameBase.Utils.Extend
{
    public static class Vector3Ex
    {
        public static Vector3 EulerToForward(this Vector3 euler)
        {
            return (Quaternion.Euler(euler) * Vector3.forward).normalized;
        }
        
        public static Quaternion EulerToQuaternion(this Vector3 euler)
        {
            return Quaternion.Euler(euler);
        }
        
        public static Vector3 ForwardToEuler(this Vector3 euler)
        {
            return Quaternion.LookRotation(euler).eulerAngles;
        }
        
        public static Quaternion ForwardToQuaternion(this Vector3 euler)
        {
            return Quaternion.LookRotation(euler);
        }
        
        public static Vector3 GetWorldOffset2D(this Vector3 offset, Vector3 forward)
        {
            forward = new Vector3(forward.x, 0, forward.z).normalized;

            return new Vector3()
            {
                x = offset.x * forward.z + offset.z * forward.x,
                y = offset.y,
                z = -offset.x * forward.x + offset.z * forward.z,
            };
        }
    }
}