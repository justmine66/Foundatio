using Foundatio.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DefaultAsyncWaitQueue<>.DebugView))]
    public sealed class DefaultAsyncWaitQueue<T> : IAsyncWaitQueue<T>
    {
        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            private readonly DefaultAsyncWaitQueue<T> _queue;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Task<T>[] Tasks
            {
                get
                {
                    List<Task<T>> list = new List<Task<T>>(_queue._queue.Count);
                    foreach (TaskCompletionSource<T> item in _queue._queue)
                    {
                        list.Add(item.Task);
                    }
                    return list.ToArray();
                }
            }

            public DebugView(DefaultAsyncWaitQueue<T> queue)
            {
                _queue = queue;
            }
        }

        private readonly Deque<TaskCompletionSource<T>> _queue = new Deque<TaskCompletionSource<T>>();

        private int Count => _queue.Count;

        bool IAsyncWaitQueue<T>.IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }

        Task<T> IAsyncWaitQueue<T>.Enqueue()
        {
            TaskCompletionSource<T> taskCompletionSource = TaskCompletionSourceExtensions.CreateAsyncTaskSource<T>();
            _queue.AddToBack(taskCompletionSource);
            return taskCompletionSource.Task;
        }

        void IAsyncWaitQueue<T>.Dequeue(T result)
        {
            _queue.RemoveFromFront().TrySetResult(result);
        }

        void IAsyncWaitQueue<T>.DequeueAll(T result)
        {
            foreach (TaskCompletionSource<T> item in _queue)
            {
                item.TrySetResult(result);
            }
            _queue.Clear();
        }

        bool IAsyncWaitQueue<T>.TryCancel(Task task, CancellationToken cancellationToken)
        {
            for (int i = 0; i != _queue.Count; i++)
            {
                if (_queue[i].Task == task)
                {
                    _queue[i].TrySetCanceled(cancellationToken);
                    _queue.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        void IAsyncWaitQueue<T>.CancelAll(CancellationToken cancellationToken)
        {
            foreach (TaskCompletionSource<T> item in _queue)
            {
                item.TrySetCanceled(cancellationToken);
            }
            _queue.Clear();
        }
    }
}
