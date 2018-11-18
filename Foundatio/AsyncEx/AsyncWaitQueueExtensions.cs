using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    public static class AsyncWaitQueueExtensions
    {
        public static Task<T> Enqueue<T>(this IAsyncWaitQueue<T> @this, object mutex, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Task.FromCanceled<T>(token);
            }
            Task<T> ret = (Task<T>)@this.Enqueue();
            if (!token.CanBeCanceled)
            {
                return (Task<T>)ret;
            }
            CancellationTokenRegistration registration = token.Register(delegate
            {
                lock (mutex)
                {
                    @this.TryCancel((Task)ret, token);
                }
            }, false);
            ((Task<T>)ret).ContinueWith((Action<Task<T>>)delegate
            {
                registration.Dispose();
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return (Task<T>)ret;
        }
    }
}
