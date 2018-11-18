using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;

namespace Foundatio.Utility
{
    public class MaintenanceBase : IDisposable
    {
        private ScheduledTimer _maintenanceTimer;

        private readonly ILoggerFactory _loggerFactory;

        protected readonly ILogger _logger;

        public MaintenanceBase(ILoggerFactory loggerFactory)
        {
            //IL_000c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0023: Unknown result type (might be due to invalid IL or missing references)
            //IL_0028: Expected O, but got Unknown
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _logger = LoggerFactoryExtensions.CreateLogger(_loggerFactory, GetType());
        }

        protected void InitializeMaintenance(TimeSpan? dueTime = default(TimeSpan?), TimeSpan? intervalTime = default(TimeSpan?))
        {
            _maintenanceTimer = new ScheduledTimer(DoMaintenanceAsync, dueTime, intervalTime, _loggerFactory);
        }

        protected void ScheduleNextMaintenance(DateTime utcDate)
        {
            _maintenanceTimer.ScheduleNext(utcDate);
        }

        protected virtual Task<DateTime?> DoMaintenanceAsync()
        {
            return Task.FromResult((DateTime?)DateTime.MaxValue);
        }

        public virtual void Dispose()
        {
            _maintenanceTimer?.Dispose();
        }
    }
}
