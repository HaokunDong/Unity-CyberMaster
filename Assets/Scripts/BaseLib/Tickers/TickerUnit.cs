using System;
using Everlasting.Extend;
using GameBase.Log;
using UnityEngine;
using UnityEngine.Profiling;

namespace GameBase.Tickers
{
    public class TickerUnit : TickerBase
    { 
        protected TickerGroup m_group;
        public TickerGroup Group => m_group;

        protected ETickOrder m_tickOrder;
        public ETickOrder TickOrder => m_tickOrder;

        protected uint m_rateBaseDeltaTimeTicks = TickersManager.FIXED_DELTA_TIME_TICKS;

        protected uint m_remainingDeltaTimeTicks = 0;

        protected Action<float> m_onUpdate = null;
        public Action<float> OnUpdate
        {
            get => m_onUpdate; set { m_onUpdate = value;
                m_profilerString = GetCallbackProfilerString(value);
            } }

        public override bool IsUpdating { get { return m_isUpdating && m_group.IsUpdating; } }

        protected string m_profilerString;

        public virtual void Init(TickerGroup group, ETickOrder tickOrder, uint rateBaseDeltaTimeTicks)
        {
            base.Init(group.TickersManager);

            m_group = group;
            m_tickOrder = tickOrder;
            m_rateBaseDeltaTimeTicks = rateBaseDeltaTimeTicks;
            
            m_remainingDeltaTimeTicks = 0;
            m_onUpdate = null;
        }

        public void Update(uint rawDeltaTimeTicks)
        {
            if(!Valid || !IsUpdating)
            {
                return;
            }

            float rateScale = m_group.RateScale;
            float timeScale = m_group.TimeScale;

            m_remainingDeltaTimeTicks += rawDeltaTimeTicks;

            uint rateDeltaTimeTicks = (uint)(m_rateBaseDeltaTimeTicks * rateScale);

            if (rateDeltaTimeTicks <= m_remainingDeltaTimeTicks)
            {
                uint deltaTimeTicks = (uint)(m_remainingDeltaTimeTicks * timeScale);

                if (deltaTimeTicks > 0)
                {
                    float deltaTime = TickerTime.TicksToSeconds(deltaTimeTicks);

                    m_remainingDeltaTimeTicks = 0;

                    DoUpdateWithTryCache(deltaTime);
                }
            }
        }

        //TODO : if Group TimeScale > 1, need more frame 
        public void FixedUpdate(uint rawDeltaTimeTicks)
        {
            if (!Valid || !IsUpdating)
            {
                return;
            }

            float timeScale = m_group.TimeScale;

            m_remainingDeltaTimeTicks += (uint) (rawDeltaTimeTicks * timeScale);
            
            if (m_rateBaseDeltaTimeTicks <= m_remainingDeltaTimeTicks)
            {
                uint deltaTimeTicks = m_rateBaseDeltaTimeTicks;
                float deltaTime = TickerTime.TicksToSeconds(deltaTimeTicks);

                m_remainingDeltaTimeTicks -= m_rateBaseDeltaTimeTicks; 

                DoUpdateWithTryCache(deltaTime);
            }
        }

        private void DoUpdateWithTryCache(float deltaTime)
        {
            try
            {
                if(!m_profilerString.IsNullOrEmpty()) Profiler.BeginSample(m_profilerString);
                DoUpdate(deltaTime);
            }
            catch (Exception e)
            {
                // LogUtils.Error($"Ticker Update报错：{e.Message}{e.StackTrace}", LogChannel.Common);
                Debug.LogException(e);
            }
            if(!m_profilerString.IsNullOrEmpty()) Profiler.EndSample();
        }
        
        protected virtual void DoUpdate(float deltaTime)
        {
            m_onUpdate?.Invoke(deltaTime);
        }

        public override void Dispose()
        {
            base.Dispose();

            m_group = null;
            m_tickOrder = 0;
            m_remainingDeltaTimeTicks = 0;
            m_onUpdate = null;
        }

        protected string GetCallbackProfilerString(Delegate action)
        {
#if DEBUG_ASSIST_ENABLE
            Profiler.BeginSample("TickerTrace(DEBUG Only)");
            string result = null;
            if (action != null)
            {
                var method = action.Method;
                result = $"{method.DeclaringType.Name} {method.Name}";
            }
            Profiler.EndSample();
            return result;
#else
            return null;
#endif
        }
    }

    public class TickerUnit_Timer : TickerUnit
    {
        private float m_interval = 0;
        private uint m_repeat = 0;

        private float m_timer = 0f;
        private uint m_repeatCount = 0;

        private Action m_onTimerUpdate = null;

        public Action OnTimerUpdate
        {
            set
            {
                m_onTimerUpdate = value;
                m_profilerString = GetCallbackProfilerString(value);
            }
        }

        private Action m_onTimerFinish = null;
        public Action OnTimerFinish { set => m_onTimerFinish = value; }

        public override void Init(TickerGroup group, ETickOrder tickOrder, uint rateBaseDeltaTimeTicks)
        {
            base.Init(group, tickOrder, rateBaseDeltaTimeTicks);

            m_interval = 0;
            m_repeat = 0;

            m_timer = 0f;
            m_repeatCount = 0;

            m_onTimerUpdate = null;
            m_onTimerFinish = null;
        }

        /// <summary></summary>
        /// <param name="interval">
        /// interval time in seconds
        /// </param>
        /// <param name="repeat">
        /// repeat times, 0 means repead forever
        /// </param>
        public void InitTimer(float interval, uint repeat)
        {
            m_interval = interval;
            m_repeat = repeat;

            m_timer = 0f;
            m_repeatCount = 0;
        }

        protected override void DoUpdate(float deltaTime)
        {
            base.DoUpdate(deltaTime);

            m_timer += deltaTime;
            if (m_timer >= m_interval)
            {
                m_timer -= m_interval;
                m_repeatCount++;

                m_onTimerUpdate?.Invoke();

                if (m_repeatCount == m_repeat)
                {
                    m_onTimerFinish?.Invoke();
                    Stop();
                }
            }

        }

        public override void Dispose()
        {
            base.Dispose();

            m_interval = 0;
            m_repeat = 0;
            m_timer = 0f;
            m_repeatCount = 0;

            m_onTimerUpdate = null;
            m_onTimerFinish = null;
    }
    }
}
