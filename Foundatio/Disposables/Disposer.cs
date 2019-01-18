using System;

namespace Foundatio.Disposables
{
    public abstract class Disposer : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void FreeManagedResources();
        protected abstract void FreeUnManagedResources();
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return; ;

            if (disposing)
            {
                // Free managed resources here.
                FreeManagedResources();
            }

            // Free unmanaged resources here.
            FreeUnManagedResources();

            _disposed = true;
        }

        ~Disposer()
        {
            Dispose(false);
        }
    }
}
