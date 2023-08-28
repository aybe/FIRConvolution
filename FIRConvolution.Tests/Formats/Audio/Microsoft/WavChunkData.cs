namespace FIRConvolution.Tests.Formats.Audio.Microsoft
{
    public sealed class WavChunkData : WavChunk
    {
        public WavChunkData()
            : base("data")
        {
        }

        public WavChunkData(Stream stream)
            : base(stream)
        {
            Position = stream.Position;

            stream.Position += ChunkSize;
        }

        public long Position { get; }
    }
}