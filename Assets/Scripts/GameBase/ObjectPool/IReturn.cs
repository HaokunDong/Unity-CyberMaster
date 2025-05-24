namespace GameBase.ObjectPool
{
    //在被缓存池回收的时候调用
    public interface IReturn
    {
        void OnReturn();
    }
}