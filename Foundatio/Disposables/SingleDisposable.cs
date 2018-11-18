using System;
using System.Threading;

namespace Foundatio.Disposables
{
    public abstract class SingleDisposable<T> : IDisposable where T : class
    {
        private T _context;

        protected SingleDisposable(T context)
        {
            _context = context;
        }

        protected abstract void Dispose(T context);

        public void Dispose()
        {
            var val = Interlocked.Exchange(ref _context, null);
            if (val != null)
            {
                Dispose(val);
            }
        }
    }
}
