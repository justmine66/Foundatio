using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Asynchronously waits for the task to complete, or for the cancellation token to be canceled.
        /// </summary>
        /// <param name="this">The task to wait for.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
        public static Task WaitAsync(this Task @this, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return @this;
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            return DoWaitAsync(@this, cancellationToken);
        }

        private static async Task DoWaitAsync(Task task, CancellationToken cancellationToken)
        {
            using (CancellationTokenTaskSource<object> cancelTaskSource = new CancellationTokenTaskSource<object>(cancellationToken))
            {
                await (await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Asynchronously waits for any of the source tasks to complete, or for the cancellation token to be canceled.
        /// </summary>
        /// <param name="this">The tasks to wait for.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
        public static Task<Task> WhenAny(this IEnumerable<Task> @this, CancellationToken cancellationToken)
        {
            return Task.WhenAny(@this).WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously waits for any of the source tasks to complete.
        /// </summary>
        /// <param name="this">The tasks to wait for.</param>
        public static Task<Task> WhenAny(this IEnumerable<Task> @this)
        {
            return Task.WhenAny(@this);
        }

        /// <summary>
        /// Asynchronously waits for all of the source tasks to complete.
        /// </summary>
        /// <param name="this">The tasks to wait for.</param>
        public static Task WhenAll(this IEnumerable<Task> @this)
        {
            return Task.WhenAll(@this);
        }

        /// <summary>
        /// Asynchronously waits for the task to complete, or for the cancellation token to be canceled.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        /// <param name="this">The task to wait for.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
        public static Task<TResult> WaitAsync<TResult>(this Task<TResult> @this, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return @this;
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<TResult>(cancellationToken);
            }
            return DoWaitAsync(@this, cancellationToken);
        }

        private static async Task<TResult> DoWaitAsync<TResult>(Task<TResult> task, CancellationToken cancellationToken)
        {
            using (CancellationTokenTaskSource<TResult> cancelTaskSource = new CancellationTokenTaskSource<TResult>(cancellationToken))
            {
                return await (await Task.WhenAny<TResult>(new Task<TResult>[2]
                {
                task,
                cancelTaskSource.Task
                }).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Asynchronously waits for any of the source tasks to complete, or for the cancellation token to be canceled.
        /// </summary>
        /// <typeparam name="TResult">The type of the task results.</typeparam>
        /// <param name="this">The tasks to wait for.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
        public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>> @this, CancellationToken cancellationToken)
        {
            return Task.WhenAny(@this).WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously waits for any of the source tasks to complete.
        /// </summary>
        /// <typeparam name="TResult">The type of the task results.</typeparam>
        /// <param name="this">The tasks to wait for.</param>
        public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>> @this)
        {
            return Task.WhenAny(@this);
        }

        /// <summary>
        /// Asynchronously waits for all of the source tasks to complete.
        /// </summary>
        /// <typeparam name="TResult">The type of the task results.</typeparam>
        /// <param name="this">The tasks to wait for.</param>
        public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> @this)
        {
            return Task.WhenAll(@this);
        }

        /// <summary>
        /// Cancellable task.
        /// </summary>
        /// <typeparam name="TResult">The type of the task results.</typeparam>
        /// <param name="this">the cancellable task.</param>
        /// <param name="token">The cancellation token that cancels the task.</param>
        /// <returns></returns>
        public static async Task<TResult> WithCancellation<TResult>(this Task<TResult> @this, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (token.Register(state =>
            {
                (state as TaskCompletionSource<object>)?.SetResult(null);
            }, tcs))
            {
                var resultTask = await Task.WhenAny(@this, tcs.Task);
                if (resultTask == tcs.Task)
                {
                    throw new OperationCanceledException(token);
                }

                return await @this;
            }
        }

        /// <summary>
        /// Cancellable task.
        /// </summary>
        /// <param name="this">the cancellable task.</param>
        /// <param name="token">The cancellation token that cancels the task.</param>
        /// <returns></returns>
        public static async Task WithCancellation(this Task @this, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (token.Register(state =>
            {
                (state as TaskCompletionSource<object>).SetResult(null);
            }, tcs))
            {
                var resultTask = await Task.WhenAny(@this, tcs.Task);
                if (resultTask == tcs.Task)
                {
                    throw new OperationCanceledException(token);
                }
            }
        }
    }
}
