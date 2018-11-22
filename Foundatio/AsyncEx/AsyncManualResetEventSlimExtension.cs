using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    /// <summary>
    /// Provides extension methods for <see cref="AsyncManualResetEventSlim" />.
    /// </summary>
    public static class AsyncManualResetEventSlimExtension
    {
        /// <summary>
        /// Blocks the current task until the current <see cref="AsyncManualResetEventSlim" /> is set, using a 32-bit signed integer to measure the time interval.
        /// </summary>
        /// <param name="this">the event.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite" />(-1) to wait indefinitely.</param>
        /// <returns>true if the <see cref="AsyncManualResetEventSlim" /> was set; otherwise, false.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1 or The number of milliseconds in <paramref name="millisecondsTimeout"/> is greater than <see cref="Int32.MaxValue"/>.</exception>
        /// <exception cref="InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
        public static Task<bool> WaitAsync(this AsyncManualResetEventSlim @this, int millisecondsTimeout)
        {
            if (@this == null)
            {
                throw new ObjectDisposedException(nameof(@this));
            }

            if (millisecondsTimeout < Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Blocks the current task until the current <see cref="AsyncManualResetEventSlim" /> is set, using a 32-bit signed integer to measure the time interval.
        /// </summary>
        /// <param name="this">the event.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"></see>(-1) to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe.</param>
        /// <returns>true if the <see cref="AsyncManualResetEventSlim" /> was set; otherwise, false.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1 or The number of milliseconds in <paramref name="millisecondsTimeout"/> is greater than <see cref="Int32.MaxValue"/>.</exception>
        /// <exception cref="InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="ObjectDisposedException">The object has already been disposed or the <see cref="CancellationTokenSource"/> that created <paramref name="cancellationToken" /> has been disposed..</exception>
        /// <exception cref="OperationCanceledException">the <paramref name="cancellationToken"/> was canceled.</exception>
        public static Task<bool> WaitAsync(this AsyncManualResetEventSlim @this, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (@this == null)
            {
                throw new ObjectDisposedException(nameof(@this));
            }

            if (millisecondsTimeout < Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Blocks the current task until the current <see cref="AsyncManualResetEventSlim"/> receives a signal, while observing a <see cref="CancellationToken" />.
        /// </summary>
        /// <param name="this">the event.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"></see> to observe.</param>
        /// <exception cref="InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// /// <exception cref="OperationCanceledException">the <paramref name="cancellationToken"/> was canceled.</exception>
        /// /// <exception cref="ObjectDisposedException">The object has already been disposed or the <see cref="CancellationTokenSource"/> that created <paramref name="cancellationToken" /> has been disposed.</exception>
        public static Task Wait(this AsyncManualResetEventSlim @this, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Blocks the current task until the current <see cref="AsyncManualResetEventSlim"/> is set, using a <see cref="TimeSpan"/> to measure the time interval.
        /// </summary>
        /// <param name="this">the event.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="Timeout.InfiniteTimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
        /// <returns>true if the <see cref="AsyncManualResetEventSlim"/> was set; otherwise, false.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, which represents an infinite time-out or The number of milliseconds in <paramref name="timeout"/> is greater than <see cref="Int32.MaxValue"/>.</exception>
        /// <exception cref="InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
        public static Task<bool> WaitAsync(this AsyncManualResetEventSlim @this, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Blocks the current task until the current <see cref="AsyncManualResetEventSlim"/> is set, using a <see cref="TimeSpan"/> to measure the time interval, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="this">the event.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="Timeout.InfiniteTimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"></see> to observe.</param>
        /// <returns>true if the <see cref="AsyncManualResetEventSlim"/> was set; otherwise, false.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, which represents an infinite time-out or The number of milliseconds in <paramref name="timeout"/> is greater than <see cref="Int32.MaxValue"/>.</exception>
        /// <exception cref="InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="ObjectDisposedException">The object has already been disposed or the <see cref="CancellationTokenSource"/> that created <paramref name="cancellationToken" /> has been disposed.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken" /> was canceled.</exception>
        public static Task<bool> WaitAsync(this AsyncManualResetEventSlim @this, TimeSpan timeout, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
