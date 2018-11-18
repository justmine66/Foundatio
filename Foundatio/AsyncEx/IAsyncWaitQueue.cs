using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    public interface IAsyncWaitQueue<T>
    {
        bool IsEmpty
        {
            get;
        }

        Task<T> Enqueue();

        void Dequeue(T result = default(T));

        void DequeueAll(T result = default(T));

        bool TryCancel(Task task, CancellationToken cancellationToken);

        void CancelAll(CancellationToken cancellationToken);
    }
}
