using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace GameBase.Tickers
{
    public class TickerGroup : TickerBase
    {
        private TickerGroup m_parentGroup;
        public TickerGroup ParentGroup => m_parentGroup;

        private uint m_fixedUpateTimeTicks = 0;
        private uint m_updateTimeTicks = 0;

        public float FixedUpdateTime => TickerTime.TicksToSeconds(m_fixedUpateTimeTicks);
        public float UpdateTime => TickerTime.TicksToSeconds(m_updateTimeTicks);

        private float m_globalRateScale = -1f;
        private float m_rateScale = 0f;
        //get absolute rate scale; set relative rate scale
        //0 means always update
        public float RateScale
        {
            get 
            {
                if (m_globalRateScale < 0f)
                {
                    float parentRateScale = m_parentGroup != null ? m_parentGroup.RateScale : 1f;
                    float rateScale = parentRateScale <= 0f ? m_rateScale : parentRateScale * m_rateScale;
                    m_globalRateScale =  rateScale <= 0f ? 0f : rateScale;
                }
                return m_globalRateScale;
            }
            set
            {
                if (m_rateScale != value)
                {
                    m_rateScale = value;
                    foreach (var tickerGroup in m_childGroupList)
                    {
                        tickerGroup.OnParentTimeScaleChange();
                    }
                }
            }
        }
        
        private float m_globalTimeScale = -1f;
        private float m_timeScale = 1f;
        //get absolute time scale; set relative time scale
        [ShowInInspector, ReadOnly]
        public float TimeScale
        {
            get 
            {
                if (m_globalTimeScale < 0f)
                {
                    float parentTimeScale = m_parentGroup != null ? m_parentGroup.TimeScale : 1f;
                    float timeScale = parentTimeScale < 0f ? m_timeScale : parentTimeScale * m_timeScale;
                    m_globalTimeScale = timeScale <= 0f ? 0f : timeScale;
                }
                return m_globalTimeScale;
            }
            set
            {
                if (m_timeScale != value)
                {
                    m_timeScale = value;
                    OnParentTimeScaleChange();
                }
            }
        }

        private void OnParentTimeScaleChange()
        {
            m_globalTimeScale = -1f;
            m_rateScale = -1f;
            foreach (var tickerGroup in m_childGroupList)
            {
                tickerGroup.OnParentTimeScaleChange();
            }
        }

        public override bool IsUpdating  
        { 
            get { return m_isUpdating && TimeScale > 0f && (m_parentGroup != null ? m_parentGroup.IsUpdating : true); } 
        }

        private List<TickerGroup> m_childGroupList = null;
        private List<TickerUnit> m_tickerUnitList = null;

        public new virtual void Init(TickersManager manager)
        {
            base.Init(manager);
            m_childGroupList = new List<TickerGroup>();
            m_tickerUnitList = new List<TickerUnit>();
        }

        public virtual void Init(TickerGroup parentGroup)
        {
            Init(parentGroup.TickersManager);
            SetParentTickerGroup(parentGroup);
        }

        private void AddChildTickerGroup(TickerGroup childToAdd)
        {
            m_childGroupList.Add(childToAdd);
        }

        private void RemoveChildTickerGroup(TickerGroup childToRemove)
        {
            int index = m_childGroupList.FindIndex((x) => x == this);
            if (index >= 0)
            {
                m_childGroupList[index] = null;
            }
        }

        private void SetParentTickerGroup(TickerGroup parentGroup)
        {
            m_parentGroup = parentGroup;
            OnParentTimeScaleChange();
        }

        public void PreUpdate(uint rawDeltaTimeTicks)
        {
            m_tickerUnitList.RemoveAll(unit => unit == null || unit.Valid);
            m_childGroupList.RemoveAll(group => group == null || group.Valid);

            //TODO : cache rate scale & time scale & isUpdating

            foreach (var group in m_childGroupList)
            {
                group.PreUpdate(rawDeltaTimeTicks);
            }
        }

        public void FixedUpdate(uint rawDeltaTimeTicks)
        {
            if(!IsUpdating)
            {
                return;
            }

            //TODO : need to figure out whether we should scale or not
            float timeScale = TimeScale;
            m_fixedUpateTimeTicks += (uint)(rawDeltaTimeTicks * timeScale);

            foreach (var group in m_childGroupList)
            {
                group.FixedUpdate(rawDeltaTimeTicks);
            }

        }

        public void Update(uint rawDeltaTimeTicks)
        {
            if(!IsUpdating)
            {
                return;
            }

            //TODO : need to figure out whether we should scale or not
            float timeScale = TimeScale;
            m_updateTimeTicks += (uint)(rawDeltaTimeTicks * timeScale);

            foreach (var group in m_childGroupList)
            {
                group.Update(rawDeltaTimeTicks);
            }
        }


        /// <summary>
        /// Update频率受Group TimeSacle影响，deltaTime不变
        /// </summary>
        public T CreateTickerUnit_FixedUpdate<T>(ETickOrder tickOrder) where T : TickerUnit, new()
        {
            T unit = new T();
            unit.Init(this, tickOrder, TickersManager.FIXED_DELTA_TIME_TICKS);
            m_tickerUnitList.Add(unit);
            m_tickersManager.RegisterTickerUnit_FixedUpdate(unit);
            return unit;
        }

        /// <summary>
        /// Update频率不受Group TimeSacle影响，deltaTime为真实时间
        /// </summary>
        public T CreateTickerUnit_Update<T>(ETickOrder tickOrder) where T : TickerUnit, new()
        {
            T unit = new T();
            unit.Init(this, tickOrder, TickersManager.FIXED_DELTA_TIME_TICKS);
            m_tickerUnitList.Add(unit);
            m_tickersManager.RegisterTickerUnit_Update(unit);
            return unit;
        }
        
        public T CreateTickerUnit_LateUpdate<T>(ETickOrder tickOrder) where T : TickerUnit, new()
        {
            T unit = new T();
            unit.Init(this, tickOrder, TickersManager.FIXED_DELTA_TIME_TICKS);
            m_tickerUnitList.Add(unit);
            m_tickersManager.RegisterTickerUnit_LateUpdate(unit);
            return unit;
        }

        public TickerGroup CreateChildTickerGroup()
        {
            TickerGroup group = new TickerGroup();
            group.Init(this);
            AddChildTickerGroup(group);
            return group;
        }

        public void ChangeParentTickerGroup(TickerGroup newParentGroup)
        {
            if (newParentGroup == m_parentGroup)
            {
                return;
            }

            if (m_parentGroup != null)
            {
                m_parentGroup.RemoveChildTickerGroup(this);
                m_parentGroup = null;
            }

            this.SetParentTickerGroup(newParentGroup);
            newParentGroup.AddChildTickerGroup(this);
        }

        public static TickerGroup CreateRootTickerGroup(TickersManager manager)
        {
            TickerGroup group = new TickerGroup();
            group.Init(manager);
            return group;
        }

        public override void Dispose()
        {
            base.Dispose();

            m_parentGroup = null;
            m_rateScale = 1;
            m_timeScale = 1f;

            foreach (var childGroup in m_childGroupList)
            {
                childGroup.Dispose();
            }
            m_childGroupList.Clear();

            foreach (var unit in m_tickerUnitList)
            {
                unit.Dispose();
            }
            m_tickerUnitList.Clear();
        }
    }
}
