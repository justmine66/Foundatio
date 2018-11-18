using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Utility
{
    public class DefaultSystemClock : ISystemClock
    {
        public static readonly DefaultSystemClock Instance = new DefaultSystemClock();

        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;

        public DateTimeOffset OffsetNow => DateTimeOffset.Now;

        public DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;

        public TimeSpan TimeZoneOffset => DateTimeOffset.Now.Offset;

        public void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        public Task SleepAsync(int milliseconds, CancellationToken ct)
        {
            return Task.Delay(milliseconds, ct);
        }
    }
}
