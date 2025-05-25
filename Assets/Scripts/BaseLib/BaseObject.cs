using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public abstract class BaseObject
    {
        protected bool m_isValid = false;
        public bool isValid
        {
            get { return m_isValid; }
        }
        protected virtual void Init() { }

        protected virtual void InitEnd() { }

        public void OnInit()
        {
            m_isValid = true;
            this.Init();
        }

        public void OnInitEnd()
        {
            this.InitEnd();
        }

        public virtual void Dispose()
        {
            m_isValid = false;
        }

        public static implicit operator bool(BaseObject obj)
        {
            return obj != null && obj.isValid;
        }
    }
}
