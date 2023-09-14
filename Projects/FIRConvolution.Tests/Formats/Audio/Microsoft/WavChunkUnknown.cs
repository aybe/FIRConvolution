using FIRConvolution.Tests.Extensions;
using JetBrains.Annotations;

namespace FIRConvolution.Tests.Formats.Audio.Microsoft
{
    [PublicAPI]
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