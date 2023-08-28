namespace FIRConvolution.Tests.Extensions
{
    internal readonly struct StreamEndiannessScope : IDisposable
    {
        private Stream Stream { get; }

        private Endianness? Endianness { get; }

        public StreamEndiannessScope(Stream stream, Endianness? endianness = null)
        {
            Endianness = (Stream = stream).SetEndianness(endianness);
        }

        void IDisposable.Dispose()
        {
            Stream.SetEndianness(Endianness);
        }
    }
}