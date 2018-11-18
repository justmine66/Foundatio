﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    /// <summary>
    /// Holds the task for a cancellation token, as well as the token registration. The registration is disposed when this instance is disposed.
    /// </summary>
    public sealed class CancellationTokenTaskSource<T> : IDisposable
    {
        /// <summary>
        /// The cancellation token registration, if any. This is <c>null</c> if the registration was not necessary.
        /// </summary>
        private readonly IDisposable _registration;

        /// <summary>
        /// Gets the task for the source cancellation token.
        /// </summary>
        public Task<T> Task
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a task for the specified cancellation token, registering with the token if necessary.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public CancellationTokenTaskSource(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Task = System.Threading.Tasks.Task.FromCanceled<T>(cancellationToken);
            }
            else
            {
                var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                _registration = cancellationToken.Register(delegate
                {
                    tcs.TrySetCanceled(cancellationToken);
                }, false);
                Task = tcs.Task; 
            }
        }

        /// <summary>
        /// Disposes the cancellation token registration, if any. Note that this may cause <see cref="P:Foundatio.AsyncEx.CancellationTokenTaskSource`1.Task" /> to never complete.
        /// </summary>
        public void Dispose()
        {
            if (_registration != null)
            {
                _registration.Dispose();
            }
        }
    }
}
