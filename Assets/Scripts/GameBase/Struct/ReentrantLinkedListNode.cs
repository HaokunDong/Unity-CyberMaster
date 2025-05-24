namespace Everlasting.Base
{
    public sealed class ReentrantLinkedListNode<T>
    {
        internal ReentrantLinkedList<T> list;
        internal ReentrantLinkedListNode<T> next;
        internal ReentrantLinkedListNode<T> prev;
        internal T item;
    
        public ReentrantLinkedListNode(T value)
        {
            this.item = value;
        }

        internal ReentrantLinkedListNode(ReentrantLinkedList<T> list, T value)
        {
            this.list = list;
            this.item = value;
        }
    
        public ReentrantLinkedList<T> List => list;

        public ReentrantLinkedListNode<T> Next => next;

        public ReentrantLinkedListNode<T> Previous => prev != null && this != list.dummyHeadNode.next ? prev : null;


        public T Value
        {
            get => item;
            set => item = value;
        }

        internal void Invalidate()
        {
            list = null;
        }
    }
}