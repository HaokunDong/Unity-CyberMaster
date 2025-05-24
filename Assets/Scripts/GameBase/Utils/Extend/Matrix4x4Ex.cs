using UnityEngine;

namespace GameBase.Utils.Extend
{
    public static class Matrix4x4Ex
    {
        //矩阵转换为位置
        public static Vector3 GetPosition(this Matrix4x4 self)
        {
            return new Vector3(self[0, 3], self[1, 3], self[2, 3]);
        }
        
        public static Vector3 GetEulerAngle(this Matrix4x4 self)
        { 
            return self.rotation.eulerAngles;
        }
        
        public static Vector3 GetForward(this Matrix4x4 self)
        {
            return self.rotation.ToForward();
        }
    }
}