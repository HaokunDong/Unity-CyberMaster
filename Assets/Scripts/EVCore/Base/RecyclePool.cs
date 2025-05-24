using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Everlasting.Base
{
    public interface IRecyclePoolNode<T>
    {
        ref T NextNode { get; }
    }

    // mutable struct, don't mark readonly.
    [StructLayout(LayoutKind.Auto)]
    public struct RecyclePool<T>
        where T : class, IRecyclePoolNode<T>
    {
        private int size;
        private T root;

        public int Size => size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            var v = root;
            if (!(v is null))
            {
                ref var nextNode = ref v.NextNode;
                root = nextNode;
                nextNode = null;
                size--;
                result = v;
                return true;
            }

            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle(T item)
        {
            item.NextNode = root;
            root = item;
            size++;
        }
    }
}