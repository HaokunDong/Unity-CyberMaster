using UnityEngine;

namespace Plugins.ParadoxNotion.CanvasCore.Extend
{
    public class BoxClass<T>
    {
        public T value;

        public BoxClass()
        {
            value = default;
        }
        
        public BoxClass(T v)
        {
            this.value = v;
        }
        
        public static implicit operator BoxClass<T>(T value)
        {
            return new BoxClass<T>(value);
        }
        
        public static implicit operator T(BoxClass<T> box)
        {
            return box.value;
        }
    }
}