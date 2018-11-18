using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Messaging
{
    public interface IMessageSubscriber
    {
        Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default(CancellationToken)) where T : class;
    }
}
