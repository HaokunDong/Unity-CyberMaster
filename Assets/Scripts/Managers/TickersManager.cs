using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace GameBase.Tickers
{
    public enum ETickOrder
    {
        INPUT = 1000,
        PRE_MOVING = 2000,
        MOVING = 3000,
        POST_MOVING = 4000,
        CAMERA = 5000,
        UI = 6000,
        EFFECT = 7000,

        INPUT_CONTROLLER = 1001,
        INPUT_CHARACTER = 1002,

        AI_SENSORSYSTEM = 1101,
        AI_SENSOR = 1102,
        AI_PERCEPTION = 1103,
        AI_PERCEPTION_GROUP = 1104,
        AI_PERCEPTION_POST = 1105,
        AI_CONTROLLER = 1106,
        AI_BEHAVIOUR_TREE_GROUP = 1107,
        AI_BEHAVIOUR_TREE = 1108,

        BATTLE = 2001,
        MATERIAL_CONTROL = 2002,

        COLLIDER = 4001,
        CLOTH_PRE = 4002,
        CLOTH = 4003,
        WATER_INTERACT_PRE = 4004
    }

    public class TickerTime
    {
        public const uint TICKS_SCALE = 100000;

        public static uint SecondsToTicks(float timeSeconds)
        {
            return (uint)(timeSeconds * TICKS_SCALE);
        }

        public static float TicksToSeconds(uint timeTicks)
        {
            return ((float)timeTicks) / TICKS_SCALE;
        }
    }

    public interface ITickersManager
    {
        TickerGroup battleGroup { get; }
        TickerGroup sceneGroup { get; }
        TickerGroup uiGroup { get; }
        TickerGroup notScaledGroup { get; }
    }

    public class TickersManager : ITickersManager
    {
        public const float FIXED_DELTA_TIME = 1f / 30f;
        public const uint FIXED_DELTA_TIME_TICKS = (uint)(FIXED_DELTA_TIME * TickerTime.TICKS_SCALE);

        private uint m_updateTimeTicks = 0;
        private uint m_fixedUpdateTimeTicks = 0;
        private uint m_lateUpdateTimeTicks = 0;

        private float m_updateTimeSeconds = 0f;
        private float m_fixedUpdateTimeSeconds = 0f;
        private float m_lateUpdateTimeSeconds = 0f;

        public uint updateTimeTicks => m_updateTimeTicks;
        public uint fixedUpdateTimeTicks => m_fixedUpdateTimeTicks;
        public uint lateUpdateTimeTicks => m_lateUpdateTimeTicks;
        
        public float updateTime => m_updateTimeSeconds;
        public float fixedUpdateTime => m_fixedUpdateTimeSeconds;
        public float lateUpdateTimeSeconds => m_lateUpdateTimeSeconds;

        private List<List<TickerUnit>> m_tickerUnitOrderedList_FixedUpdate = null;
        private List<List<TickerUnit>> m_tickerUnitOrderedList_Update = null;
        private List<List<TickerUnit>> m_tickerUnitOrderedList_LateUpdate = null;
        
        private List<KeyValuePair<ETickOrder, TickerUnit>> m_addTickerUnitPendingList_FixedUpdate = null;
        private List<KeyValuePair<ETickOrder, TickerUnit>> m_addTickerUnitPendingList_Update = null;
        private List<KeyValuePair<ETickOrder, TickerUnit>> m_addTickerUnitPendingList_LateUpdate = null;

        //key: tickorder idx; value: idx
        private List<KeyValuePair<int, int>> m_removeTickerUnitPendingList_Update = null;
        private List<KeyValuePair<int, int>> m_removeTickerUnitPendingList_FixedUpdate = null;
        private List<KeyValuePair<int, int>> m_removeTickerUnitPendingList_LateUpdate = null;
        private Dictionary<ETickOrder, int> m_tickOrderIdxDict = null;

        private TickerGroup m_rootGroupForUpdate = null;
        private TickerGroup m_rootGroupForLateUpdate = null;
        private TickerGroup m_uiGroup = null;
        private TickerGroup m_battleGroup = null;
        private TickerGroup m_sceneGroup = null;
        private TickerGroup m_notScaledGroup = null;

        public TickerGroup RootGroupForUpdate => m_rootGroupForUpdate;
        public TickerGroup RootGroupForLateUpdate => m_rootGroupForLateUpdate;

        public TickerGroup uiGroup => m_uiGroup;
        public TickerGroup battleGroup => m_battleGroup;
        //all scene logics & views
        public TickerGroup sceneGroup => m_sceneGroup;

        /// <summary>
        /// 不会被改变Scale的Group，禁止设置Scale
        /// </summary>
        public TickerGroup notScaledGroup => m_notScaledGroup;

        public float SceneTime => m_sceneGroup.UpdateTime;
        public float SceneFixedTime => m_sceneGroup.FixedUpdateTime;

        private int GetIdxFromTickOrder(ETickOrder tickOrder)
        {
            return m_tickOrderIdxDict[tickOrder];
        }

        public void Init()
        {
            m_rootGroupForUpdate = TickerGroup.CreateRootTickerGroup(this);
            m_uiGroup = m_rootGroupForUpdate.CreateChildTickerGroup();
            m_battleGroup = m_rootGroupForUpdate.CreateChildTickerGroup();
            m_battleGroup.Pause(); //战斗tickerGroup只有在战斗中才打开
            m_sceneGroup = m_rootGroupForUpdate.CreateChildTickerGroup();
            m_notScaledGroup = m_rootGroupForUpdate.CreateChildTickerGroup();

            m_rootGroupForLateUpdate = TickerGroup.CreateRootTickerGroup(this);

            m_tickerUnitOrderedList_FixedUpdate = new List<List<TickerUnit>>();
            m_tickerUnitOrderedList_Update = new List<List<TickerUnit>>();
            m_tickerUnitOrderedList_LateUpdate = new List<List<TickerUnit>>();

            m_addTickerUnitPendingList_FixedUpdate = new List<KeyValuePair<ETickOrder, TickerUnit>>();
            m_addTickerUnitPendingList_Update = new List<KeyValuePair<ETickOrder, TickerUnit>>();
            m_addTickerUnitPendingList_LateUpdate = new List<KeyValuePair<ETickOrder, TickerUnit>>();

            m_removeTickerUnitPendingList_FixedUpdate = new List<KeyValuePair<int, int>>();
            m_removeTickerUnitPendingList_Update = new List<KeyValuePair<int, int>>();
            m_removeTickerUnitPendingList_LateUpdate = new List<KeyValuePair<int, int>>();

            ETickOrder[]  tickOrders = (ETickOrder[])Enum.GetValues(typeof(ETickOrder));
            m_tickOrderIdxDict = new Dictionary<ETickOrder, int>(tickOrders.Length);
            

            for (int i = 0; i < tickOrders.Length; i++)
            {
                //Debug.Log($"Creating tick order: {order}");
                m_tickerUnitOrderedList_FixedUpdate.Add(new List<TickerUnit>());
                m_tickerUnitOrderedList_Update.Add(new List<TickerUnit>());
                m_tickerUnitOrderedList_LateUpdate.Add(new List<TickerUnit>());
                m_tickOrderIdxDict.Add(tickOrders[i], i);
            }

            //m_battleGroup?.Start();
        }

        public void Update(float deltaTime)
        {
            uint deltaTimeTicks = TickerTime.SecondsToTicks(deltaTime);
            uint currentTimeTicks = m_updateTimeTicks + deltaTimeTicks;
            //update groups recursively

            Profiler.BeginSample("TickGroup_PreUpdate");
            m_rootGroupForUpdate.PreUpdate(deltaTimeTicks);
            Profiler.EndSample();
            //fixed update
            {
                while (m_fixedUpdateTimeTicks <= currentTimeTicks)
                {
                    m_fixedUpdateTimeTicks += FIXED_DELTA_TIME_TICKS;
                    m_fixedUpdateTimeSeconds = TickerTime.TicksToSeconds(m_fixedUpdateTimeTicks);

                    Profiler.BeginSample("TickGroup_FixedUpdate");
                    m_rootGroupForUpdate.FixedUpdate(FIXED_DELTA_TIME_TICKS);
                    Profiler.EndSample();

                    Profiler.BeginSample("TickUnit_FixedUpdate");
                    for(int i = 0; i < m_tickerUnitOrderedList_FixedUpdate.Count; i++)
                    {
                        var unitList = m_tickerUnitOrderedList_FixedUpdate[i];

                        for (int j = 0; j < unitList.Count; j++)
                        {
                            var unit = unitList[j];
                            if (unit.Valid)
                            {
                                unit.FixedUpdate(FIXED_DELTA_TIME_TICKS);
                            }
                            else
                            {
                                m_removeTickerUnitPendingList_FixedUpdate.Add(new KeyValuePair<int, int>(i, j));
                            }
                        }
                    }
                    Profiler.EndSample();

                    for (int i = m_removeTickerUnitPendingList_FixedUpdate.Count - 1; i >= 0; i--)
                    {
                        int subListIdx = m_removeTickerUnitPendingList_FixedUpdate[i].Key;
                        int unitIdx = m_removeTickerUnitPendingList_FixedUpdate[i].Value;

                        if (subListIdx < m_tickerUnitOrderedList_FixedUpdate.Count && unitIdx < m_tickerUnitOrderedList_FixedUpdate[subListIdx].Count)
                        {
                            m_tickerUnitOrderedList_FixedUpdate[subListIdx].RemoveAt(unitIdx);
                        }
                    }
                    m_removeTickerUnitPendingList_FixedUpdate.Clear();

                    foreach (var unitKV in m_addTickerUnitPendingList_FixedUpdate)
                    {
                        m_tickerUnitOrderedList_FixedUpdate[GetIdxFromTickOrder(unitKV.Key)].Add(unitKV.Value);
                    }
                    m_addTickerUnitPendingList_FixedUpdate.Clear();

                }
            }

            //update
            {
                m_updateTimeTicks = currentTimeTicks;
                m_updateTimeSeconds = TickerTime.TicksToSeconds(m_updateTimeTicks);

                Profiler.BeginSample("TickGroup_Update");
                m_rootGroupForUpdate.Update(deltaTimeTicks);
                Profiler.EndSample();

                Profiler.BeginSample("TickUnit_Update");
                for (int i = 0; i < m_tickerUnitOrderedList_Update.Count; i++)
                {
                    var unitList = m_tickerUnitOrderedList_Update[i];

                    for (int j = 0; j < unitList.Count; j++)
                    {
                        var unit = unitList[j];
                        if (unit.Valid)
                        {
                            unit.Update(deltaTimeTicks);
                        }
                        else
                        {
                            m_removeTickerUnitPendingList_Update.Add(new KeyValuePair<int, int>(i, j));
                        }
                    }
                }
                Profiler.EndSample();

                for (int i = m_removeTickerUnitPendingList_Update.Count - 1; i >= 0; i--)
                {
                    var subListIdx = m_removeTickerUnitPendingList_Update[i].Key;
                    var unitIdx = m_removeTickerUnitPendingList_Update[i].Value;

                    if (subListIdx < m_tickerUnitOrderedList_Update.Count && unitIdx < m_tickerUnitOrderedList_Update[subListIdx].Count)
                    {
                        m_tickerUnitOrderedList_Update[subListIdx].RemoveAt(unitIdx);
                    }
                }
                m_removeTickerUnitPendingList_Update.Clear();

                foreach (var unitKV in m_addTickerUnitPendingList_Update)
                {
                    m_tickerUnitOrderedList_Update[GetIdxFromTickOrder(unitKV.Key)].Add(unitKV.Value);
                }
                m_addTickerUnitPendingList_Update.Clear();
            }

        }

        public void LateUpdate(float deltaTime)
        {
            uint deltaTimeTicks = TickerTime.SecondsToTicks(deltaTime);
            m_lateUpdateTimeTicks += deltaTimeTicks;
            m_lateUpdateTimeSeconds = TickerTime.TicksToSeconds(m_lateUpdateTimeTicks);

            Profiler.BeginSample("TickGroup_LateUpdate");
            m_rootGroupForLateUpdate.PreUpdate(deltaTimeTicks);
            m_rootGroupForLateUpdate.Update(deltaTimeTicks);
            Profiler.EndSample();
            
            foreach (var unitKV in m_addTickerUnitPendingList_LateUpdate)
            {
                m_tickerUnitOrderedList_LateUpdate[GetIdxFromTickOrder(unitKV.Key)].Add(unitKV.Value);
            }

            Profiler.BeginSample("TickUnit_LateUpdate");
            for (int i = 0; i < m_tickerUnitOrderedList_LateUpdate.Count; i++)
            {
                var unitList = m_tickerUnitOrderedList_LateUpdate[i];

                for (int j = 0; j < unitList.Count; j++)
                {
                    var unit = unitList[j];
                    if (unit.Valid)
                    {
                        unit.Update(deltaTimeTicks);
                    }
                    else
                    {
                        m_removeTickerUnitPendingList_LateUpdate.Add(new KeyValuePair<int, int>(i, j));
                    }
                }
            }
            Profiler.EndSample();

            for (int i = m_removeTickerUnitPendingList_LateUpdate.Count - 1; i >= 0; i--)
            {
                var subListIdx = m_removeTickerUnitPendingList_LateUpdate[i].Key;
                var unitIdx = m_removeTickerUnitPendingList_LateUpdate[i].Value;

                if (subListIdx < m_tickerUnitOrderedList_LateUpdate.Count && unitIdx < m_tickerUnitOrderedList_LateUpdate[subListIdx].Count)
                {
                    m_tickerUnitOrderedList_LateUpdate[subListIdx].RemoveAt(unitIdx);
                }
            }

            m_removeTickerUnitPendingList_LateUpdate.Clear();
            m_addTickerUnitPendingList_LateUpdate.Clear();
        }
        
        
        /// <summary>
        /// do not call this function directly
        /// </summary>
        /// <param name="unit"></param>
        public void RegisterTickerUnit_FixedUpdate(TickerUnit unit)
        {
            m_addTickerUnitPendingList_FixedUpdate.Add(new KeyValuePair<ETickOrder, TickerUnit>(unit.TickOrder, unit));
        }
        /// <summary>
        /// do not call this function directly
        /// </summary>
        /// <param name="unit"></param>
        public void RegisterTickerUnit_Update(TickerUnit unit)
        {
            m_addTickerUnitPendingList_Update.Add(new KeyValuePair<ETickOrder, TickerUnit>(unit.TickOrder, unit));
        }

        /// <summary>
        /// do not call this function directly
        /// </summary>
        /// <param name="unit"></param>
        public void RegisterTickerUnit_LateUpdate(TickerUnit unit)
        {
            m_addTickerUnitPendingList_LateUpdate.Add(new KeyValuePair<ETickOrder, TickerUnit>(unit.TickOrder, unit));
        }

    }
}
