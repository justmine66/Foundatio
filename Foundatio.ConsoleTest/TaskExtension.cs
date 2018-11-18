using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.ConsoleTest
{
    public static class TaskExtension
    {
        public static async Task<T> WithCancellation<T>(this Task<T> @this, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
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

                return await @this;
            }
        }
    }
}
