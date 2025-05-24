using UnityEngine;

namespace GameBase.Utils.Extend
{
    public static class QuaternionEx
    {
        public static Vector3 ToForward(this Quaternion quaternion)
        {
            return (quaternion * Vector3.forward).normalized;
        }
    }
}