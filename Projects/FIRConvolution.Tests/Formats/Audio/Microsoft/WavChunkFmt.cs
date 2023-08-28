using FIRConvolution.Tests.Extensions;

namespace FIRConvolution.Tests.Formats.Audio.Microsoft
{
    public sealed class WavChunkFmt : WavChunk
    {
        public WavChunkFmt()
            : base("fmt ", 16)
        {
        }

        public WavChunkFmt(Stream stream)
            : base(stream)
        {
            if (ChunkSize != 16)
            {
                throw new NotSupportedException();
            }

            Compression   = stream.Read<WavCompression>();
            Channels      = stream.ReadUInt16();
            SampleRate    = stream.ReadUInt32();
            ByteRate      = stream.ReadUInt32();
            BlockAlign    = stream.ReadUInt16();
            BitsPerSample = stream.ReadUInt16();
        }

        public WavCompression Compression { get; init; }

        public ushort Channels { get; init; }

        public uint SampleRate { get; init; }

        public uint ByteRate { get; init; }

        public ushort BlockAlign { get; init; }

        public ushort BitsPerSample { get; init; }

        public override void Write(Stream stream)
        {
            base.Write(stream);

            using var scope = stream.SetEndiannessScope(Endianness.LittleEndian);

            stream.Write(Compression);
            stream.Write(Channels);
            stream.Write(SampleRate);
            stream.Write(ByteRate);
            stream.Write(BlockAlign);
            stream.Write(BitsPerSample);
        }

        public static ushort GetBlockAlign(ushort channels, ushort bitsPerSample)
        {
            var blockAlign = (ushort)(bitsPerSample / 8 * channels);

            return blockAlign;
        }

        public static uint GetByteRate(ushort channels, ushort bitsPerSample, uint sampleRate)
        {
            var blockAlign = GetBlockAlign(channels, bitsPerSample);

            var byteRate = sampleRate * blockAlign;

            return byteRate;
        }
    }
}