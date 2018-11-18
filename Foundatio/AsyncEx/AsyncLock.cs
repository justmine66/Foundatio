using Foundatio.AsyncEx.Synchronous;
using Foundatio.Disposables;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    /// <summary>
    /// A mutual exclusion lock that is compatible with async. Note that this lock is not recursive!
    /// </summary>
    [DebuggerDisplay("Id = {Id}, Taken = {_taken}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncLock
    {
        private sealed class Key : SingleDisposable<AsyncLock>
        {
            public Key(AsyncLock asyncLock)
                : base(asyncLock)
            {
            }

            protected override void Dispose(AsyncLock context)
            {
                context.ReleaseLock();
            }
        }

        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncLock _mutex;

            public int Id => _mutex.Id;

            public bool Taken => _mutex._taken;

            public IAsyncWaitQueue<IDisposable> WaitQueue => _mutex._queue;

            public DebugView(AsyncLock mutex)
            {
                _mutex = mutex;
            }
        }

        private bool _taken;

        private readonly IAsyncWaitQueue<IDisposable> _queue;

        private int _id;

        private readonly object _mutex;

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous lock.
        /// </summary>
        public int Id => IdManager<AsyncLock>.GetId(ref _id);

        /// <summary>
        /// Creates a new async-compatible mutual exclusion lock.
        /// </summary>
        public AsyncLock()
            : this(null)
        {
        }

        /// <summary>
        /// Creates a new async-compatible mutual exclusion lock using the specified wait queue.
        /// </summary>
        /// <param name="queue">The wait queue used to manage waiters. This may be null to use a default (FIFO)</param>
        public AsyncLock(IAsyncWaitQueue<IDisposable> queue)
        {
            _queue = (queue ?? new DefaultAsyncWaitQueue<IDisposable>());
            _mutex = new object();
        }

        private Task<IDisposable> RequestLockAsync(CancellationToken cancellationToken)
        {
            lock (_mutex)
            {
                if (_taken)
                {
                    return _queue.Enqueue(_mutex, cancellationToken);
                }
                _taken = true;
                return Task.FromResult((IDisposable)new Key(this));
            }
        }

        /// <summary>
        /// Asynchronously acquires the lock. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> LockAsync(CancellationToken cancellationToken)
        {
            return new AwaitableDisposable<IDisposable>(RequestLockAsync(cancellationToken));
        }

        /// <summary>
        /// Asynchronously acquires the lock. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> LockAsync()
        {
            return LockAsync(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously acquires the lock. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public IDisposable Lock(CancellationToken cancellationToken)
        {
            return RequestLockAsync(cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Synchronously acquires the lock. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public IDisposable Lock()
        {
            return Lock(CancellationToken.None);
        }

        internal void ReleaseLock()
        {
            lock (_mutex)
            {
                if (_queue.IsEmpty)
                {
                    _taken = false;
                }
                else
                {
                    _queue.Dequeue(new Key(this));
                }
            }
        }
    }
}
