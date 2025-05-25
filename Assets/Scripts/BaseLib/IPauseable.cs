namespace GameBase
{
    public interface IPauseable
    {
        bool IsLiving { get; }
        void Pause();
        void Resume();
        void OnPause();
        void OnResume();

        ///////////////////// Template ///////////////////// 

        //public bool IsLiving { get; private set; }

        //public void Pause()
        //{
        //    if (IsLiving)
        //    {
        //        IsLiving = false;
        //        OnPause();
        //    }
        //}

        //public void Resume()
        //{
        //    if (!IsLiving)
        //    {
        //        IsLiving = true;
        //        OnResume();
        //    }
        //}

        //public virtual void OnPause()
        //{
        //}

        //public virtual void OnResume()
        //{
        //}
    }
}
