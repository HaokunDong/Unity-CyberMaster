namespace GameBase.Tickers
{
    public abstract class TickerBase
    {
        protected TickersManager m_tickersManager = null;
        public TickersManager TickersManager => m_tickersManager;

        private bool m_valid = false;
        public bool Valid
        {
            get { return m_valid; }
        }

        protected bool m_isAutoDispose = true;
        public bool IsAutoDispose => m_isAutoDispose;

        protected bool m_isUpdating = false;
        public virtual bool IsUpdating
        {
            get { return m_isUpdating; }
        }

        public virtual void Start()
        {
            m_isUpdating = true;
        }

        public virtual void Pause()
        {
            m_isUpdating = false;
        }

        public virtual void Stop()
        {
            m_isUpdating = false;
            if (m_isAutoDispose)
            {
                Dispose();
            }
        }

        protected virtual void Init(TickersManager manager)
        {
            m_valid = true;
            m_tickersManager = manager;
            m_isUpdating = true;
        }

        public virtual void Dispose()
        {
            m_valid = false;
            m_tickersManager = null;
            m_isUpdating = false;
        }
    } 
}
