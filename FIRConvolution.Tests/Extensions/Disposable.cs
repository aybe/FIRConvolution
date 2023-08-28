using JetBrains.Annotations;

namespace FIRConvolution.Tests.Extensions
{
    public abstract class Disposable : IDisposable
    {
        private bool Disposed { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Disposable()
        {
            Dispose(false);
        }

        [PublicAPI]
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeManaged();
            }

            DisposeNative();

            Disposed = true;
        }

        [PublicAPI]
        protected virtual void DisposeManaged()
        {
        }

        [PublicAPI]
        protected virtual void DisposeNative()
        {
        }
    }
}