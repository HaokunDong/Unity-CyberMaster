using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.ParadoxNotion.CanvasCore.Extend
{
    //用于替代event，仅用于单线程
    //原生event为了线程安全，内部使用了不可变array，每次+=或者-=会产生300B左右gc
    public class DelegateList<T> : IEnumerable<T> where T : Delegate
    {
        private List<T> m_actionList;
        public List<T> ActionList => m_actionList;

        public static DelegateList<T> operator +(DelegateList<T> list, T action)
        {
            list ??= new DelegateList<T>();
            list.m_actionList ??= new List<T>();
            list.m_actionList.Add(action);
            return list;
        }
        
        public static DelegateList<T> operator -(DelegateList<T> list, T action)
        {
            if (list?.m_actionList != null)
            {
                int index = list.m_actionList.IndexOf(action);
                if (index > -1)
                {
                    if(list.m_foreachIndex > -1 && index <= list.m_foreachIndex) list.m_foreachIndex--;
                    list.m_actionList.RemoveAt(index);
                }
            }
            return list;
        }

        private int m_foreachIndex = -1;
        //支持循环时AddRemove
        //不能嵌套
        public EnumeratorSupportAddRemove GetEnumeratorSupportAddRemoveDuring()
        {
            if (m_actionList == null) throw new NullReferenceException();
            //m_actionList
            if (m_foreachIndex > -1)
            {
                throw new Exception("DelegateList ForeachSupportAddRemoveDuring 不支持嵌套");
            }

            return new EnumeratorSupportAddRemove(this);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            m_actionList ??= new List<T>();
            return m_actionList.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return m_actionList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_actionList.GetEnumerator();
        }

        public struct EnumeratorSupportAddRemove : IEnumerator<T>
        {
            private DelegateList<T> list;
            private T current;

            internal EnumeratorSupportAddRemove(DelegateList<T> list)
            {
                this.list = list;
                list.m_foreachIndex = 0;
                this.current = null;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                DelegateList<T> list = this.list;
                if (list.m_actionList == null || list.m_foreachIndex >= list.m_actionList.Count)
                {
                    list.m_foreachIndex = -1;
                    this.current = null;
                    return false;
                }
                this.current = list.m_actionList[list.m_foreachIndex];
                ++list.m_foreachIndex;
                return true;
            }

            public T Current => this.current;

            object IEnumerator.Current => (object)this.Current;

            void IEnumerator.Reset()
            {
                list.m_foreachIndex = 0;
                this.current = default(T);
            }
        }
    }
}