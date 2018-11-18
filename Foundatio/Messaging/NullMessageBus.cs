using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Messaging
{
    public class NullMessageBus : IMessageBus, IMessagePublisher, IMessageSubscriber, IDisposable
    {
        public void Dispose() { }

        public Task PublishAsync(Type messageType, object message, TimeSpan? delay = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.CompletedTask;
        }
    }
}
