using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Utility
{
    /// <summary>
    /// Abstracts the system clock to unified time.
    /// </summary>
    public interface ISystemClock
    {
        DateTime Now { get; }

        /// <summary>
        /// Retrieves the current system time in UTC.
        /// </summary>
        /// <returns></returns>
        DateTime UtcNow { get; }

        DateTimeOffset OffsetNow { get; }

        DateTimeOffset OffsetUtcNow { get; }

        void Sleep(int milliseconds);

        Task SleepAsync(int milliseconds, CancellationToken ct);

        TimeSpan TimeZoneOffset { get; }
    }
}
