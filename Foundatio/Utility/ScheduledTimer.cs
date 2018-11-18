using Foundatio.AsyncEx;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Utility
{
    public class ScheduledTimer : IDisposable
    {
        private DateTime _next = DateTime.MaxValue;

        private DateTime _last = DateTime.MinValue;

        private readonly Timer _timer;

        private readonly ILogger _logger;

        private readonly Func<Task<DateTime?>> _timerCallback;

        private readonly TimeSpan _minimumInterval;

        private readonly AsyncLock _lock = new AsyncLock();

        private bool _isRunning;

        private bool _shouldRunAgainImmediately;

        public ScheduledTimer(Func<Task<DateTime?>> timerCallback, TimeSpan? dueTime = default(TimeSpan?), TimeSpan? minimumIntervalTime = default(TimeSpan?), ILoggerFactory loggerFactory = null)
        {
            _logger = ((loggerFactory != null) ? LoggerFactoryExtensions.CreateLogger<ScheduledTimer>(loggerFactory) : null) ?? (NullLogger<ScheduledTimer>.Instance);
            _timerCallback = timerCallback ?? throw new ArgumentNullException("timerCallback");
            _minimumInterval = (minimumIntervalTime ?? TimeSpan.Zero);
            var dueTime2 = dueTime.HasValue ? ((int)dueTime.Value.TotalMilliseconds) : (-1);
            _timer = new Timer(delegate
            {
                RunCallbackAsync().AnyContext().GetAwaiter().GetResult();
            }, null, dueTime2, -1);
        }

        public void ScheduleNext(DateTime? utcDate = default(DateTime?))
        {
            DateTime utcNow = SystemClock.UtcNow;
            if (!utcDate.HasValue || utcDate.Value < utcNow)
            {
                utcDate = utcNow;
            }
            bool flag = _logger.IsEnabled(0);
            if (flag)
            {
                LoggerExtensions.LogTrace(_logger, "ScheduleNext called: value={NextRun:O}", new object[1]
                {
                utcDate.Value
                });
            }
            DateTime? d = utcDate;
            DateTime value = DateTime.MaxValue;
            if (d == (DateTime?)value)
            {
                if (flag)
                {
                    LoggerExtensions.LogTrace(_logger, "Ignoring MaxValue", Array.Empty<object>());
                }
            }
            else
            {
                if (_next > utcNow)
                {
                    d = utcDate;
                    value = _next;
                    if (d > (DateTime?)value)
                    {
                        if (flag)
                        {
                            ILogger logger = _logger;
                            object[] obj = new object[2];
                            value = utcDate.Value;
                            obj[0] = value.Ticks;
                            obj[1] = _next.Ticks;
                            LoggerExtensions.LogTrace(logger, "Ignoring because already scheduled for earlier time: {PreviousTicks} Next: {NextTicks}", obj);
                        }
                        return;
                    }
                }
                value = _next;
                d = utcDate;
                if ((DateTime?)value == d)
                {
                    if (flag)
                    {
                        LoggerExtensions.LogTrace(_logger, "Ignoring because already scheduled for same time", Array.Empty<object>());
                    }
                }
                else
                {
                    using (_lock.Lock())
                    {
                        if (!(_next > utcNow))
                        {
                            goto IL_01dd;
                        }
                        d = utcDate;
                        value = _next;
                        if (!(d > (DateTime?)value))
                        {
                            goto IL_01dd;
                        }
                        if (flag)
                        {
                            ILogger logger2 = _logger;
                            object[] obj2 = new object[2];
                            value = utcDate.Value;
                            obj2[0] = value.Ticks;
                            obj2[1] = _next.Ticks;
                            LoggerExtensions.LogTrace(logger2, "Ignoring because already scheduled for earlier time: {PreviousTicks} Next: {NextTicks}", obj2);
                        }
                        goto end_IL_0163;
                        IL_01dd:
                        value = _next;
                        d = utcDate;
                        if ((DateTime?)value == d)
                        {
                            if (flag)
                            {
                                LoggerExtensions.LogTrace(_logger, "Ignoring because already scheduled for same time", Array.Empty<object>());
                            }
                        }
                        else
                        {
                            value = utcDate.Value;
                            int num = Math.Max((int)Math.Ceiling(value.Subtract(utcNow).TotalMilliseconds), 0);
                            _next = utcDate.Value;
                            if (_last == DateTime.MinValue)
                            {
                                _last = _next;
                            }
                            if (flag)
                            {
                                LoggerExtensions.LogTrace(_logger, "Scheduling next: delay={Delay}", new object[1]
                                {
                                num
                                });
                            }
                            _timer.Change(num, -1);
                        }
                        end_IL_0163:;
                    }
                }
            }
        }

        private async Task RunCallbackAsync()
        {
            bool isTraceLogLevelEnabled = _logger.IsEnabled(0);
            if (_isRunning)
            {
                if (isTraceLogLevelEnabled)
                {
                    _logger.LogTrace("Exiting run callback because its already running, will run again immediately.", Array.Empty<object>());
                }
                _shouldRunAgainImmediately = true;
            }
            else
            {
                if (isTraceLogLevelEnabled)
                {
                    LoggerExtensions.LogTrace(_logger, "Starting RunCallbackAsync", Array.Empty<object>());
                }
                using (await _lock.LockAsync().AnyContext())
                {
                    if (_isRunning)
                    {
                        if (isTraceLogLevelEnabled)
                        {
                            _logger.LogTrace("Exiting run callback because its already running, will run again immediately.", Array.Empty<object>());
                        }
                        _shouldRunAgainImmediately = true;
                        return;
                    }
                    _last = SystemClock.UtcNow;
                }
                try
                {
                    _isRunning = true;
                    DateTime? next = null;
                    Stopwatch sw = Stopwatch.StartNew();
                    try
                    {
                        next = await _timerCallback().AnyContext();
                    }
                    catch (Exception ex)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            LoggerExtensions.LogError(_logger, ex, "Error running scheduled timer callback: {Message}", new object[1]
                            {
                            ex.Message
                            });
                        }
                        _shouldRunAgainImmediately = true;
                    }
                    finally
                    {
                        sw.Stop();
                        if (isTraceLogLevelEnabled)
                        {
                            LoggerExtensions.LogTrace(_logger, "Callback took: {Elapsed:g}", new object[1]
                            {
                            sw.Elapsed
                            });
                        }
                    }
                    if (_minimumInterval > TimeSpan.Zero)
                    {
                        if (isTraceLogLevelEnabled)
                        {
                            LoggerExtensions.LogTrace(_logger, "Sleeping for minimum interval: {Interval:g}", new object[1]
                            {
                            _minimumInterval
                            });
                        }
                        await SystemClock.SleepAsync(_minimumInterval, default(CancellationToken)).AnyContext();
                        if (isTraceLogLevelEnabled)
                        {
                            LoggerExtensions.LogTrace(_logger, "Finished sleeping", Array.Empty<object>());
                        }
                    }
                    DateTime dateTime = SystemClock.UtcNow.AddMilliseconds(10.0);
                    if (_shouldRunAgainImmediately || (next.HasValue && next.Value <= dateTime))
                    {
                        ScheduleNext(dateTime);
                    }
                    else if (next.HasValue)
                    {
                        ScheduleNext(next.Value);
                    }
                }
                catch (Exception ex2)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        LoggerExtensions.LogError(_logger, ex2, "Error running schedule next callback: {Message}", new object[1]
                        {
                        ex2.Message
                        });
                    }
                }
                finally
                {
                    _isRunning = false;
                    _shouldRunAgainImmediately = false;
                }
                if (isTraceLogLevelEnabled)
                {
                    LoggerExtensions.LogTrace(_logger, "Finished RunCallbackAsync", Array.Empty<object>());
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
