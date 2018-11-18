using Foundatio.AsyncEx.Synchronous;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    public static class TaskCompletionSourceExtensions
    {
        public static bool TryCompleteFromCompletedTask<TResult, TSourceResult>(this TaskCompletionSource<TResult> @this, Task<TSourceResult> task) where TSourceResult : TResult
        {
            if (task.IsFaulted)
            {
                return @this.TrySetException((IEnumerable<Exception>)task.Exception.InnerExceptions);
            }
            if (task.IsCanceled)
            {
                try
                {
                    task.WaitAndUnwrapException();
                }
                catch (OperationCanceledException ex)
                {
                    CancellationToken cancellationToken = ex.CancellationToken;
                    return cancellationToken.IsCancellationRequested ? @this.TrySetCanceled(cancellationToken) : @this.TrySetCanceled();
                }
            }
            return @this.TrySetResult((TResult)(object)task.Result);
        }

        public static bool TryCompleteFromCompletedTask<TResult>(this TaskCompletionSource<TResult> @this, Task task, Func<TResult> resultFunc)
        {
            if (task.IsFaulted)
            {
                return @this.TrySetException((IEnumerable<Exception>)task.Exception.InnerExceptions);
            }
            if (task.IsCanceled)
            {
                try
                {
                    task.WaitAndUnwrapException();
                }
                catch (OperationCanceledException ex)
                {
                    CancellationToken cancellationToken = ex.CancellationToken;
                    return cancellationToken.IsCancellationRequested ? @this.TrySetCanceled(cancellationToken) : @this.TrySetCanceled();
                }
            }
            return @this.TrySetResult(resultFunc());
        }

        public static TaskCompletionSource<TResult> CreateAsyncTaskSource<TResult>()
        {
            return new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
