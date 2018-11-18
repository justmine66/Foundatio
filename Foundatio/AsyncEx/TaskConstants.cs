using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.AsyncEx
{
    public static class TaskConstants
    {
        private static readonly Task<bool> booleanTrue = Task.FromResult(true);

        private static readonly Task<int> intNegativeOne = Task.FromResult(-1);

        public static Task<bool> BooleanTrue => booleanTrue;

        public static Task<bool> BooleanFalse => TaskConstants<bool>.Default;

        public static Task<int> Int32Zero => TaskConstants<int>.Default;

        public static Task<int> Int32NegativeOne => intNegativeOne;

        public static Task Completed => Task.CompletedTask;

        public static Task Canceled => TaskConstants<object>.Canceled;
    }

    public static class TaskConstants<T>
    {
        private static readonly Task<T> defaultValue = Task.FromResult<T>(default(T));

        private static readonly Task<T> canceled = Task.FromCanceled<T>(new CancellationToken(true));

        public static Task<T> Default => defaultValue;

        public static Task<T> Canceled => canceled;
    }
}
