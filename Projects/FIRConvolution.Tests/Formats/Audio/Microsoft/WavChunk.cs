using FIRConvolution.Tests.Extensions;
using JetBrains.Annotations;

namespace FIRConvolution.Tests.Formats.Audio.Microsoft
{
    [PublicAPI]
    public abstract class WavChunk
    {
        protected WavChunk(FourCC chunkId, uint chunkSize = default)
        {
            ChunkId   = chunkId;
            ChunkSize = chunkSize;
        }

        protected WavChunk(Stream stream)
        {
            ChunkId   = new FourCC(stream);
            ChunkSize = stream.ReadUInt32(Endianness.LittleEndian);
        }

        public FourCC ChunkId { get; }

        public uint ChunkSize { get; set; }

        public static WavChunk ReadChunk(Stream stream)
        {
            var peek = stream.Peek(s => new FourCC(s));

            var name = peek.ToString();

            return name switch
            {
                "RIFF" => new WavChunkRiff(stream),
                "data" => new WavChunkData(stream),
                "fmt " => new WavChunkFmt(stream),
                _      => new WavChunkUnknown(stream)
            };
        }

        public override string ToString()
        {
            return $"'{ChunkId}', {ChunkSize}";
        }

        public virtual void Write(Stream stream)
        {
            using (stream.SetEndiannessScope(Endianness.LittleEndian))
            {
                stream.Write(ChunkId);
                stream.Write(ChunkSize);
            }
        }
    }
}