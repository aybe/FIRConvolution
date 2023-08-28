using FIRConvolution.Tests.Extensions;

namespace FIRConvolution.Tests.Formats.Audio.Microsoft
{
    public sealed class WavChunkRiff : WavChunk
    {
        public WavChunkRiff()
            : base("RIFF")
        {
        }

        public WavChunkRiff(Stream stream)
            : base(stream)
        {
            var type = new FourCC(stream);

            if (type != "WAVE")
            {
                throw new NotSupportedException();
            }

            Type = type;
        }

        private FourCC Type { get; } = "WAVE";

        public override void Write(Stream stream)
        {
            base.Write(stream);

            using (stream.SetEndiannessScope(Endianness.LittleEndian))
            {
                stream.Write(Type);
            }
        }
    }
}