using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Utility
{
    public static class SystemClock
    {
        private static ISystemClock _instance = DefaultSystemClock.Instance;

        public static ISystemClock Instance
        {
            get
            {
                return _instance ?? DefaultSystemClock.Instance;
            }
            set
            {
                _instance = value;
            }
        }

        public static DateTime Now => Instance.Now;

        public static DateTime UtcNow => Instance.UtcNow;

        public static DateTimeOffset OffsetNow => Instance.OffsetNow;

        public static DateTimeOffset OffsetUtcNow => Instance.OffsetUtcNow;

        public static TimeSpan TimeZoneOffset => Instance.TimeZoneOffset;

        public static void Sleep(TimeSpan time)
        {
            Instance.Sleep((int)time.TotalMilliseconds);
        }

        public static void Sleep(int milliseconds)
        {
            Instance.Sleep(milliseconds);
        }

        public static Task SleepAsync(TimeSpan time, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Instance.SleepAsync((int)time.TotalMilliseconds, cancellationToken);
        }

        public static Task SleepAsync(int milliseconds, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Instance.SleepAsync(milliseconds, cancellationToken);
        }
    }

}
