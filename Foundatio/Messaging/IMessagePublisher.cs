using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishAsync(Type messageType, object message, TimeSpan? delay = default(TimeSpan?), CancellationToken cancellationToken = default(CancellationToken));
    }
}
