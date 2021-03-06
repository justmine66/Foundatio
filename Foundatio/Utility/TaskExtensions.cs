﻿using Foundatio.AsyncEx;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Foundatio.Utility
{
    internal static class TaskExtensions
    {
        [DebuggerStepThrough]
        public static ConfiguredTaskAwaitable<TResult> AnyContext<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false);
        }

        [DebuggerStepThrough]
        public static ConfiguredTaskAwaitable AnyContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        [DebuggerStepThrough]
        public static ConfiguredTaskAwaitable<TResult> AnyContext<TResult>(this AwaitableDisposable<TResult> task) where TResult : IDisposable
        {
            return task.ConfigureAwait(false);
        }
    }
}
