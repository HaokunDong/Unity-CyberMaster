using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Everlasting.Base.Threading
{
    public class TaskQueue
    {
        private readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();
        private volatile int _delegateQueuedOrRunning = 0;

        public void Enqueue(Action job)
        {
            queue.Enqueue(job);
            if (Interlocked.CompareExchange(ref _delegateQueuedOrRunning, 1, 0) == 0)
            {
                Task.Run(ProcessQueuedItems);
            }
        }
        public bool IsEmpty => queue.IsEmpty;

        private void ProcessQueuedItems()
        {
            Action item;
            if (!queue.TryDequeue(out item))
            {
                Interlocked.Exchange(ref _delegateQueuedOrRunning, 0);
                return;
            }

            Task.Run(item).ContinueWith((t) =>
            {
                ProcessQueuedItems();
            });
        }

    }
}