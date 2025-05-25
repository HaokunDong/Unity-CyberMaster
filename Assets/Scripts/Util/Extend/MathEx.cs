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
    }
}