using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    /// <summary>
    /// Provides a slimmed down version of <see cref="AsyncManualResetEvent"/>.
    /// </summary>
    /// <remarks>
    /// All public members of <see cref="AsyncManualResetEventSlim"/> are thread-safe and may be used concurrently from multiple threads, with the exception of Dispose, which must only be used when all other operations on the <see cref="AsyncManualResetEventSlim"/> have completed, and Reset, which should only be used when no other threads are accessing the event.
    /// </remarks>
    [DebuggerDisplay("Set = {IsSet}")]
    public class AsyncManualResetEventSlim : IDisposable
    {
        private volatile TaskCompletionSource<bool> _tcs;
        private volatile int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncManualResetEventSlim"/> class with an initial state of nonSignaled.
        /// </summary>
        public AsyncManualResetEventSlim() : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncManualResetEventSlim"/> class with a Boolean value indicating whether to set the initial state to signaled.
        /// </summary>
        /// <param name="initialState">true to set the initial state signaled; false to set the initial state to nonSignaled.</param>
        public AsyncManualResetEventSlim(bool initialState)
        {
            _tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<bool>();
            if (initialState)
            {
                _tcs.SetResult(true);
            }
        }

        /// <summary>
        /// Blocks the current thread until the current <see cref="AsyncManualResetEventSlim"></see> is set.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
        /// <exception cref="InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        public Task WaitAsync()
        {
            if (_tcs == null)
            {
                throw new ObjectDisposedException(nameof(_tcs));
            }

            return _tcs.Task;
        }

        /// <summary>
        /// Sets the state of the event to signaled, which allows one or more threads waiting on the event to proceed.
        /// </summary>
        public void Set()
        {
            var tcs = _tcs;
            Task.Factory.StartNew(s => ((TaskCompletionSource<bool>)s).TrySetResult(true),
                tcs, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
            tcs.Task.Wait();
        }

        /// <summary>
        /// Sets the state of the event to nonSignaled, which causes threads to block.
        /// </summary>
        /// <remarks>
        /// The goal is to make the Tasks returned from subsequent calls to WaitAsync not completed, so we need to swap <see cref="_tcs"/> in a new <see cref="TaskCompletionSource{TResult}"/>. In doing so, though, we need to make sure that, if multiple threads are calling <see cref="Reset"/>, <see cref="Set"/>, and ，<see cref="WaitAsync"/> concurrently, no Tasks returned from <see cref="WaitAsync"/> are orphaned (meaning that we would not want someone to call <see cref="WaitAsync"/> and get back a Task that won’t be completed the next time someone calls Set). To achieve that, we’ll make sure to only swap in a new Task if the current one is already completed, and we’ll make sure that we do the swap atomically.
        /// </remarks>
        public void Reset()
        {
            while (true)
            {
                var tcs = _tcs;
                if (!_tcs.Task.IsCompleted || Interlocked.CompareExchange(ref _tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Releases the managed resources used by <see cref="AsyncManualResetEventSlim"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region [ internal members ]

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tcs = null;
            }
        }

        #endregion
    }
}
