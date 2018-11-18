using System.Threading;

namespace Foundatio.AsyncEx
{
    internal static class IdManager<TTag>
    {
        private static int _lastId;

        public static int GetId(ref int id)
        {
            if (id != 0)
            {
                return id;
            }
            int num;
            do
            {
                num = Interlocked.Increment(ref _lastId);
            }
            while (num == 0);
            Interlocked.CompareExchange(ref id, num, 0);
            return id;
        }
    }
}
