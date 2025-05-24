
namespace Everlasting.Base
{
    public class Singleton<T> where T:class, new()
    {
        private static readonly T instance = new T();

        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        protected Singleton()
        {
            if (instance != null)
            {
                throw new System.Exception("one instance only");
            }
        }


    }
}
