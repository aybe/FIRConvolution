using System.Runtime.InteropServices;
using FIRConvolution.Tests.Extensions;
using FIRConvolution.Tests.Formats.Audio.Extensions;

namespace FIRConvolution.Tests.Formats.Audio.Microsoft
{
    public sealed class Wav : Disposable
    {
        public Wav(in Stream stream)
        {
            Stream         = stream;
            StreamPosition = stream.Position;
            StreamReader   = true;

            CheckReader();

            ReadChunks();

            ChunkRiff = Chunks.Single<WavChunkRiff>();
            ChunkFmt  = Chunks.Single<WavChunkFmt>();
            ChunkData = Chunks.Single<WavChunkData>();

            Stream.Position = ChunkData.Position;
        }

        public Wav(in Stream stream, in ushort channels, in ushort bitsPerSample, in uint sampleRate)
        {
            Stream         = stream;
            StreamPosition = stream.Position;
            StreamWriter   = true;

            CheckWriter();

            ChunkRiff = new WavChunkRiff();

            ChunkFmt = new WavChunkFmt
            {
                Compression   = bitsPerSample is 32 ? WavCompression.IEEEFloat : WavCompression.PCM,
                Channels      = channels,
                SampleRate    = sampleRate,
                ByteRate      = WavChunkFmt.GetByteRate(channels, bitsPerSample, sampleRate),
                BlockAlign    = WavChunkFmt.GetBlockAlign(channels, bitsPerSample),
                BitsPerSample = bitsPerSample
            };

            ChunkData = new WavChunkData();

            Chunks.Add(ChunkRiff);
            Chunks.Add(ChunkFmt);
            Chunks.Add(ChunkData);

            WriteChunks();
        }

        private Stream Stream { get; }

        private long StreamPosition { get; }

        private bool StreamReader { get; }

        private bool StreamWriter { get; }

        private List<WavChunk> Chunks { get; } = new();

        private WavChunkRiff ChunkRiff { get; }

        private WavChunkFmt ChunkFmt { get; }

        private WavChunkData ChunkData { get; }

        public ushort BitsPerSample => ChunkFmt.BitsPerSample;

        public ushort Channels => ChunkFmt.Channels;

        public uint SampleRate => ChunkFmt.SampleRate;

        protected override void DisposeManaged()
        {
            if (StreamReader)
            {
            }

            if (StreamWriter)
            {
                var position = Stream.Position;

                var length = position - StreamPosition;

                ChunkRiff.ChunkSize = (uint)(length - 8);

                ChunkData.ChunkSize = (uint)(length - 44);

                Stream.Position = StreamPosition;

                WriteChunks();

                Stream.Flush();
            }
        }

        private void CheckBuffer<T>(in Span<T> buffer, in int index, in int count) where T : unmanaged
        {
            if (index < 0 || index >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index + count * ChunkFmt.Channels > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        private int CheckLength(in int length)
        {
            var bytes = ChunkData.Position + ChunkData.ChunkSize - Stream.Position;

            var samples = Converters.BytesToSamples(bytes, ChunkFmt.BitsPerSample, ChunkFmt.Channels);

            var min = Math.Min(length, samples);

            return (int)min;
        }

        private void CheckReader()
        {
            if (Stream.CanRead == false)
            {
                throw new InvalidOperationException();
            }
        }

        private void CheckWriter()
        {
            if (Stream.CanWrite == false)
            {
                throw new InvalidOperationException();
            }
        }

        public T[] CreateBuffer<T>(in int count) where T : unmanaged
        {
            var buffer = new T[count * ChunkFmt.Channels];

            return buffer;
        }

        private static BufferInfo CreateBufferInfo<T>(in Span<T> buffer) where T : unmanaged
        {
            var span16 = MemoryMarshal.Cast<T, short>(buffer);
            var span32 = MemoryMarshal.Cast<T, float>(buffer);
            var type16 = typeof(T) == typeof(short);
            var type32 = typeof(T) == typeof(float);

            return new BufferInfo(span16, span32, type16, type32);
        }

        public int Read<T>(in T[] buffer) where T : unmanaged
        {
            return Read(buffer.AsSpan());
        }

        public int Read<T>(in T[] buffer, int index, int count) where T : unmanaged
        {
            return Read(buffer.AsSpan(), index, count);
        }

        public int Read<T>(in Span<T> buffer) where T : unmanaged
        {
            var read = Read(buffer, 0, buffer.Length / ChunkFmt.Channels);

            return read;
        }

        public int Read<T>(in Span<T> buffer, in int index, in int count) where T : unmanaged
        {
            CheckBuffer(buffer, index, count);

            var (span16, span32, type16, type32) = CreateBufferInfo(buffer);

            var length = CheckLength(count);

            for (var i = index; i < index + length * ChunkFmt.Channels; i++)
            {
                switch (ChunkFmt.BitsPerSample)
                {
                    case 16:
                    {
                        var sample = Read16();

                        if (type16)
                        {
                            span16[i] = sample;
                        }
                        else if (type32)
                        {
                            span32[i] = Converters.To32Bit(sample);
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        break;
                    }
                    case 32:
                    {
                        var sample = Read32();

                        if (type16)
                        {
                            span16[i] = Converters.To16Bit(sample);
                        }
                        else if (type32)
                        {
                            span32[i] = sample;
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        break;
                    }
                    default:
                        throw new NotSupportedException();
                }
            }

            return length;
        }

        private short Read16()
        {
            var value = Stream.ReadInt16(Endianness.LittleEndian);

            return value;
        }

        private float Read32()
        {
            var value = Stream.ReadSingle(Endianness.LittleEndian);

            return value;
        }

        private void ReadChunks()
        {
            while (Stream.Position < Stream.Length)
            {
                var chunk = WavChunk.ReadChunk(Stream);

                Chunks.Add(chunk);
            }
        }

        public void Write<T>(in T[] buffer) where T : unmanaged
        {
            Write(buffer.AsSpan());
        }

        public void Write<T>(in T[] buffer, in int index, in int count) where T : unmanaged
        {
            Write(buffer.AsSpan(), index, count);
        }

        public void Write<T>(in Span<T> buffer) where T : unmanaged
        {
            Write(buffer, 0, buffer.Length / ChunkFmt.Channels);
        }

        public void Write<T>(in Span<T> buffer, in int index, in int count) where T : unmanaged
        {
            CheckBuffer(buffer, index, count);

            var (span16, span32, type16, type32) = CreateBufferInfo(buffer);

            var length = count * ChunkFmt.Channels;

            for (var i = index; i < index + length; i++)
            {
                switch (ChunkFmt.BitsPerSample)
                {
                    case 16:
                        if (type16)
                        {
                            Write16(span16[i]);
                        }
                        else if (type32)
                        {
                            Write16(Converters.To16Bit(span32[i]));
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        break;
                    case 32:
                        if (type16)
                        {
                            Write32(Converters.To32Bit(span16[i]));
                        }
                        else if (type32)
                        {
                            Write32(span32[i]);
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private void Write16(in short value)
        {
            Stream.Write(value, Endianness.LittleEndian);
        }

        private void Write32(in float value)
        {
            Stream.Write(value, Endianness.LittleEndian);
        }

        private void WriteChunks()
        {
            foreach (var chunk in Chunks)
            {
                chunk.Write(Stream);
            }
        }

        private readonly ref struct BufferInfo
        {
            public BufferInfo(Span<short> span16, Span<float> span32, bool type16, bool type32)
            {
                Span16 = span16;
                Span32 = span32;
                Type16 = type16;
                Type32 = type32;
            }

            public Span<short> Span16 { get; }

            public Span<float> Span32 { get; }

            public bool Type16 { get; }

            public bool Type32 { get; }

            public void Deconstruct(out Span<short> span16, out Span<float> span32, out bool type16, out bool type32)
            {
                span16 = Span16;
                span32 = Span32;
                type16 = Type16;
                type32 = Type32;
            }
        }
    }
}