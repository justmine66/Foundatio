using Foundatio.AsyncEx;
using Foundatio.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Queues
{
    public class TaskQueue : IDisposable
    {
        private readonly ConcurrentQueue<Func<Task>> _queue = new ConcurrentQueue<Func<Task>>();

        private readonly SemaphoreSlim _semaphore;

        private readonly AsyncAutoResetEvent _autoResetEvent = new AsyncAutoResetEvent();

        private CancellationTokenSource _workLoopCancellationTokenSource;

        private readonly int _maxItems;

        private int _working;

        private readonly Action _queueEmptyAction;

        private readonly ILogger _logger;

        public int Queued => _queue.Count;

        public int Working => _working;

        public TaskQueue(int maxItems = int.MaxValue, byte maxDegreeOfParallelism = 1, bool autoStart = true, Action queueEmptyAction = null, ILoggerFactory loggerFactory = null)
        {
            _maxItems = maxItems;
            _queueEmptyAction = queueEmptyAction;
            _semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            _logger = (loggerFactory != null ? LoggerFactoryExtensions.CreateLogger<TaskQueue>(loggerFactory) : null) ?? (NullLogger<TaskQueue>.Instance);
            if (autoStart)
            {
                Start(default(CancellationToken));
            }
        }

        public bool Enqueue(Func<Task> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }
            if (_queue.Count >= _maxItems)
            {
                LoggerExtensions.LogError(_logger, "Ignoring queued task: Queue is full", Array.Empty<object>());
                return false;
            }
            _queue.Enqueue(task);
            _autoResetEvent.Set();
            return true;
        }

        public void Start(CancellationToken token = default(CancellationToken))
        {
            _workLoopCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            StartWorking();
        }

        private void StartWorking()
        {
            bool isTraceLogLevelEnabled = _logger.IsEnabled(0);
            if (isTraceLogLevelEnabled)
            {
                LoggerExtensions.LogTrace(_logger, "Starting worker loop.", Array.Empty<object>());
            }
            Func<Task> task;
            Task.Run(async delegate
            {
                while (!_workLoopCancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        bool flag = await _semaphore.WaitAsync(1000, _workLoopCancellationTokenSource.Token).AnyContext();
                        if (!_queue.TryDequeue(out task))
                        {
                            if (flag)
                            {
                                _semaphore.Release();
                            }
                            if (_queue.IsEmpty)
                            {
                                if (isTraceLogLevelEnabled)
                                {
                                    LoggerExtensions.LogTrace(_logger, "Waiting to dequeue task.", Array.Empty<object>());
                                }
                                try
                                {
                                    using (CancellationTokenSource timeoutCancellationTokenSource = new CancellationTokenSource(10000))
                                    {
                                        using (CancellationTokenSource dequeueCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_workLoopCancellationTokenSource.Token, timeoutCancellationTokenSource.Token))
                                        {
                                            await _autoResetEvent.WaitAsync(dequeueCancellationTokenSource.Token).AnyContext();
                                        }
                                    }
                                }
                                catch (OperationCanceledException)
                                {
                                }
                            }
                        }
                        else
                        {
                            Interlocked.Increment(ref _working);
                            if (isTraceLogLevelEnabled)
                            {
                                LoggerExtensions.LogTrace(_logger, "Running dequeued task", Array.Empty<object>());
                            }
                            Task.Run(() => task(), _workLoopCancellationTokenSource.Token).ContinueWith(delegate (Task t)
                            {
                                Interlocked.Decrement(ref _working);
                                _semaphore.Release();
                                if (t.IsFaulted)
                                {
                                    Exception innerException2 = t.Exception.InnerException;
                                    LoggerExtensions.LogError(_logger, innerException2, "Error running dequeue task: {Message}", new object[1]
                                    {
                                    innerException2?.Message
                                    });
                                }
                                else if (t.IsCanceled)
                                {
                                    LoggerExtensions.LogWarning(_logger, "Dequeue task was cancelled.", Array.Empty<object>());
                                }
                                else if (isTraceLogLevelEnabled)
                                {
                                    LoggerExtensions.LogTrace(_logger, "Finished running dequeued task.", Array.Empty<object>());
                                }
                                if (_queueEmptyAction != null && _working == 0 && _queue.IsEmpty && _queue.Count == 0)
                                {
                                    if (isTraceLogLevelEnabled)
                                    {
                                        LoggerExtensions.LogTrace(_logger, "Running completed action..", Array.Empty<object>());
                                    }
                                    _queueEmptyAction();
                                }
                            });
                        }
                    }
                    catch (OperationCanceledException ex2)
                    {
                        LoggerExtensions.LogWarning(_logger, (Exception)ex2, "Worker loop was cancelled.", Array.Empty<object>());
                    }
                    catch (Exception ex3)
                    {
                        LoggerExtensions.LogError(_logger, ex3, "Error running worker loop: {Message}", new object[1]
                        {
                        ex3.Message
                        });
                    }
                }
            }, _workLoopCancellationTokenSource.Token).ContinueWith(delegate (Task t)
            {
                CancellationToken token;
                if (t.IsFaulted)
                {
                    Exception innerException = t.Exception.InnerException;
                    LoggerExtensions.LogError(_logger, innerException, "Worker loop exiting: {Message}", new object[1]
                    {
                    innerException.Message
                    });
                }
                else
                {
                    if (!t.IsCanceled)
                    {
                        token = _workLoopCancellationTokenSource.Token;
                        if (!token.IsCancellationRequested)
                        {
                            LoggerExtensions.LogCritical(_logger, "Worker loop finished prematurely.", Array.Empty<object>());
                            goto IL_0093;
                        }
                    }
                    LoggerExtensions.LogTrace(_logger, "Worker loop was cancelled.", Array.Empty<object>());
                }
                goto IL_0093;
                IL_0093:
                token = _workLoopCancellationTokenSource.Token;
                if (!token.IsCancellationRequested)
                {
                    StartWorking();
                }
            });
        }

        public void Dispose()
        {
            LoggerExtensions.LogTrace(_logger, "Disposing", Array.Empty<object>());
            _workLoopCancellationTokenSource?.Cancel();
            ConcurrentQueueExtensions.Clear(_queue);
        }
    }
}
