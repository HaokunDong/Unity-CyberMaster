using System;
using System.Collections.Generic;
using Managers;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameBase.Tickers
{
    //可控制频率的TickerUnit，用于降频优化
    public class TickerUnitTimeFrequency : TickerUnit
    {
        //多少时间运行一次，0表示每帧都运行
        public float TimeRunSpacing { get; set; } = 0.1f;
        
        //未执行的deltaTime
        private float m_remainDeltaTime = 0f;
        private float m_remainDeltaTimeOffset = 0f;
        
        //给个初始time，防止Action都挤在一帧中执行
        private static float s_defaultTime = 0f;
        public static float GetDefaultTime(float spacingTime)
        {
            if (spacingTime <= 0) throw new ArgumentException();
            s_defaultTime += spacingTime * 0.1f;
            while (s_defaultTime > spacingTime)
            {
                s_defaultTime -= spacingTime;
            }
            return s_defaultTime;
        }

        public void Init(float spacingTime = 0.1f)
        {
            TimeRunSpacing = spacingTime;
            m_remainDeltaTimeOffset = GetDefaultTime(spacingTime);
        }
        
        protected override void DoUpdate(float deltaTime)
        {
            m_remainDeltaTime += deltaTime;
            if (m_remainDeltaTime + m_remainDeltaTimeOffset > TimeRunSpacing)
            {
                // BeforeUpdate();
                base.DoUpdate(m_remainDeltaTime);
                m_remainDeltaTimeOffset += m_remainDeltaTime;
                m_remainDeltaTime = 0f;
                while (m_remainDeltaTimeOffset > TimeRunSpacing)
                {
                    m_remainDeltaTimeOffset -= TimeRunSpacing;
                }
            }
        }

        // protected virtual void BeforeUpdate() { }
    }

    //根据距离决定频率
    // public class TickerUnitTimeFrequencyByDistance : TickerUnitTimeFrequency
    // {
    //     public struct DistanceSpacingParam
    //     {
    //         public float distance;
    //         public float runSpacing;
    //
    //         public DistanceSpacingParam(float distance, float runSpacing)
    //         {
    //             this.distance = distance;
    //             this.runSpacing = runSpacing;
    //         }
    //     }
    //     
    //     //默认参数
    //     //distance必须从小到大
    //     public static List<DistanceSpacingParam> defaultParams = new List<DistanceSpacingParam>()
    //     {
    //         new DistanceSpacingParam(30f, 0.1f),
    //         new DistanceSpacingParam(float.MaxValue, 1f),
    //     };
    //     
    //     //transform必须要有
    //     public Func<Vector3> getModelPos;
    //     public List<DistanceSpacingParam> distanceParams = defaultParams;
    //
    //     public bool enabled = true;
    //     protected override void BeforeUpdate()
    //     {
    //         if (enabled)
    //         {
    //             var playerPos = DataSetManager.s_sceneMainRoleCharacterDataset.Position;
    //             var distanceSqr = (getModelPos() - playerPos).sqrMagnitude;
    //             for (var i = 0; i < distanceParams.Count; i++)
    //             {
    //                 var param = distanceParams[i];
    //                 if (distanceSqr < param.distance * param.distance)
    //                 {
    //                     TimeRunSpacing = param.runSpacing;
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    // }
}