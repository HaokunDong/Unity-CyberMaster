using System;
using UnityEngine;

namespace Tools
{
    public static class MathEx
    {
        public const float Sqrt3 = 1.732050807f;

        public static int SecondToMillisecondInt(this float time)
        {
            return Mathf.FloorToInt(time* 1000);
        }
        
        public static TimeSpan SecondToTimeSpan(this float time)
        {
            return TimeSpan.FromSeconds(time);
        }

        /// <summary>
        /// 计算旋转矩形的4个顶点
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="size">宽高</param>
        /// <param name="rotationDegrees">旋转角度（度）</param>
        /// <returns>按顺时针顺序返回四个顶点</returns>
        public static Vector2[] GetRotatedRectVertices(Vector2 center, Vector2 size, float rotationDegrees)
        {
            Vector2 halfSize = size * 0.5f;

            // 原始局部坐标（未旋转）
            Vector2[] localCorners = new Vector2[4]
            {
            new Vector2(-halfSize.x, -halfSize.y), // 左下
            new Vector2(-halfSize.x,  halfSize.y), // 左上
            new Vector2( halfSize.x,  halfSize.y), // 右上
            new Vector2( halfSize.x, -halfSize.y), // 右下
            };

            float rad = rotationDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            Vector2[] worldCorners = new Vector2[4];

            for (int i = 0; i < 4; i++)
            {
                // 旋转并平移回世界坐标
                Vector2 p = localCorners[i];
                float x = p.x * cos - p.y * sin;
                float y = p.x * sin + p.y * cos;
                worldCorners[i] = new Vector2(x, y) + center;
            }

            return worldCorners;
        }
    }
}