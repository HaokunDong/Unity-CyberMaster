using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Everlasting.Base
{
    public class ReentrantLinkedList<T> : ICollection<T>, ICollection, IReadOnlyCollection<T>
    {
        internal readonly ReentrantLinkedListNode<T> dummyHeadNode;
        internal ReentrantLinkedListNode<T> tailNode;
        internal int count;
        private object _syncRoot;

        public ReentrantLinkedList()
        {
            dummyHeadNode = tailNode = new ReentrantLinkedListNode<T>(this, default);
        }

        public ReentrantLinkedListNode<T> AddLast(T value)
        {
            var newNode = new ReentrantLinkedListNode<T>(this, value);
            InternalAppendNode(newNode);
            return newNode;
        }
        
        public void AddLast(ReentrantLinkedListNode<T> value)
        {
            InternalAppendNode(value);
        }
        
        public ReentrantLinkedListNode<T> AddFirst(T value)
        {
            var newNode = new ReentrantLinkedListNode<T>(this, value);
            InternalInsertNodeBefore(dummyHeadNode.next, newNode);
            return newNode;
        }
        
        public void AddFirst(ReentrantLinkedListNode<T> value)
        {
            InternalInsertNodeBefore(dummyHeadNode.next, value);
        }
        
        public ReentrantLinkedListNode<T> Find(T value)
        {
            var linkedListNode = dummyHeadNode.next;
            var equalityComparer = EqualityComparer<T>.Default;

            if (value != null)
            {
                while (linkedListNode != null && !equalityComparer.Equals(linkedListNode.item, value))
                {
                    linkedListNode = linkedListNode.next;
                }
            }
            else
            {
                while (linkedListNode != null && linkedListNode.item != null)
                {
                    linkedListNode = linkedListNode.next;
                }
            }

            return linkedListNode;
        }

        public ReentrantLinkedListNode<T> Find(Predicate<T> predicate)
        {
            var linkedListNode = dummyHeadNode.next;
            while (linkedListNode != null && !predicate(linkedListNode.item))
            {
                linkedListNode = linkedListNode.next;
            }

            return linkedListNode;
        }

        public T First => dummyHeadNode.next != null ? dummyHeadNode.next.item : default;
        public T Last => tailNode != dummyHeadNode ? tailNode.item : default;

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void Add(T item)
        {
            AddLast(item);
        }

        public void Clear()
        {
            var linkedListNode = dummyHeadNode.next;
            while (linkedListNode != null)
            {
                var tempNode = linkedListNode;
                linkedListNode = linkedListNode.Next;
                tempNode.Invalidate();
            }
            
            dummyHeadNode.next = null;
            tailNode = dummyHeadNode;
            count = 0;
        }

        public bool Contains(T item)
        {
            return Find(item) != null;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof (array));
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof (arrayIndex), "IndexOutOfRange " + arrayIndex);
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("InsufficientSpace");
            var linkedListNode = dummyHeadNode.next;
            if (linkedListNode == null)
                return;
            do
            {
                array[arrayIndex++] = linkedListNode.item;
                linkedListNode = linkedListNode.next;
            }
            while (linkedListNode != null);
        }

        public bool Remove(T item)
        {
            var node = Find(item);
            if (node == null)
                return false;
            InternalRemoveNode(node);
            return true;
        }

        public bool Remove(ReentrantLinkedListNode<T> node)
        {
            if (node == null || node.list != this)
                return false;
            InternalRemoveNode(node);
            return true;
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        private void InternalAppendNode(ReentrantLinkedListNode<T> newNode)
        {
            tailNode.next = newNode;
            newNode.list = this;
            newNode.prev = tailNode;
            newNode.next = null;
            tailNode = newNode;
            ++count;
        }

        private void InternalInsertNodeBefore(ReentrantLinkedListNode<T> node, ReentrantLinkedListNode<T> newNode)
        {
            if (node == null || (node == tailNode && tailNode == dummyHeadNode))
            {
                InternalAppendNode(newNode);
            }
            else
            {
                newNode.list = this;
                newNode.next = node;
                newNode.prev = node.prev;
                node.prev.next = newNode;
                node.prev = newNode;
                ++count;
            }
        }
        
        internal void InternalRemoveNode(ReentrantLinkedListNode<T> node)
        {
            if (node == tailNode)
            {
                node.prev.next = null;
                tailNode = node.prev;
            }
            else
            {
                node.next.prev = node.prev;
                node.prev.next = node.next;
            }

            node.Invalidate();
            --count;
        }

        public int Count => count;

        public bool IsSynchronized => false;

        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object) null);
                return _syncRoot;
            }
        }

        public bool IsReadOnly => false;

        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private ReentrantLinkedList<T> list;
            private ReentrantLinkedListNode<T> node;
            // private bool first;
            private T current;
            
            public Enumerator(ReentrantLinkedList<T> list)
            {
                this.list = list;
                node = list.dummyHeadNode.next;
                // this.first = true;
                current = default;
            }
            
            public bool MoveNext()
            {
                while (node is { list: null })
                {
                    node = node.next;
                }

                if (node == null)
                {
                    return false;
                }
                
                current = node.item;
                node = node.next;
                return true;
            }

            public void Reset()
            {
                // this.first = true;
                node = null;
            }

            object IEnumerator.Current => Current;

            public T Current => current;

            public void Dispose()
            {
            }
        }
    }
}