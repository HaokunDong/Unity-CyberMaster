using System;
using System.Collections.Generic;

namespace GameBase.Message
{
    //DataSet的轻量级实现，一个简单的数据驱动机制，便于对现有框架进行改造
    public struct DataBind<T>
    {
        private T m_value;

        public T Value
        {
            get => m_value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(m_value, value))
                {
                    m_value = value;
                    SendBindEvent();
                }
            }
        }

        public DataBind(T initValue)
        {
            m_value = initValue;
            m_callbacks = null;
        }

        [NonSerialized]
        private List<Action<T>> m_callbacks;

        public Action<T> BindHandler(Action<T> callback, bool callImmediate = true)
        {
            m_callbacks ??= new List<Action<T>>();
            m_callbacks.Add(callback);
            if(callImmediate) callback(m_value);
            return callback;
        }

        //由绑定端保证释放
        public void UnBindHandler(Action<T> callback)
        {
            m_callbacks?.Remove(callback);
        }

        private void SendBindEvent()
        {
            if (m_callbacks == null) return;
            for (var i = 0; i < m_callbacks.Count; i++)
            {
                m_callbacks[i].Invoke(m_value);
            }
        }
        
        public static implicit operator T(DataBind<T> bind)
        {
            return bind.Value;
        }
    }
}