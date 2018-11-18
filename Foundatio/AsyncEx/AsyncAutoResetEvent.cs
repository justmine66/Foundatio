using Foundatio.AsyncEx.Synchronous;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    [DebuggerDisplay("Id = {Id}, IsSet = {_set}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncAutoResetEvent
    {
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncAutoResetEvent _are;

            public int Id => _are.Id;

            public bool IsSet => _are._set;

            public IAsyncWaitQueue<object> WaitQueue => _are._queue;

            public DebugView(AsyncAutoResetEvent are)
            {
                _are = are;
            }
        }

        private readonly IAsyncWaitQueue<object> _queue;

        private bool _set;

        private int _id;

        private readonly object _mutex;

        public int Id => IdManager<AsyncAutoResetEvent>.GetId(ref _id);

        public bool IsSet
        {
            get
            {
                lock (_mutex)
                {
                    return _set;
                }
            }
        }

        public AsyncAutoResetEvent(bool set, IAsyncWaitQueue<object> queue)
        {
            _queue = (queue ?? new DefaultAsyncWaitQueue<object>());
            _set = set;
            _mutex = new object();
        }

        public AsyncAutoResetEvent(bool set)
            : this(set, null)
        {
        }

        public AsyncAutoResetEvent()
            : this(false, null)
        {
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            lock (_mutex)
            {
                if (!_set)
                {
                    return _queue.Enqueue(_mutex, cancellationToken);
                }
                _set = false;
                return TaskConstants.Completed;
            }
        }

        public Task WaitAsync()
        {
            return WaitAsync(CancellationToken.None);
        }

        public void Wait(CancellationToken cancellationToken)
        {
            WaitAsync(cancellationToken).WaitAndUnwrapException();
        }

        public void Wait()
        {
            Wait(CancellationToken.None);
        }

        public void Set()
        {
            lock (_mutex)
            {
                if (_queue.IsEmpty)
                {
                    _set = true;
                }
                else
                {
                    _queue.Dequeue(null);
                }
            }
        }
    }
}
