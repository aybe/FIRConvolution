using FIRConvolution.Tests.Extensions;

namespace FIRConvolution.Tests.Formats.Audio.Microsoft
{
    public sealed class WavChunkUnknown : WavChunk
    {
        public WavChunkUnknown(Stream reader)
            : base(reader)
        {
            Data = reader.ReadBytes((int)ChunkSize);
        }

        public byte[] Data { get; }
    }
}