using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace FIRConvolution.Tests.Extensions
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public static class StreamEndiannessExtensions
    {
        public static Endianness Endianness { get; } =
            BitConverter.IsLittleEndian
                ? Endianness.LittleEndian
                : Endianness.BigEndian;

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private static ConcurrentDictionary<Stream, Endianness?> Endiannesses { get; } = new();

        public static Endianness? GetEndianness(this Stream stream)
        {
            var endianness = Endiannesses.GetOrAdd(stream, default(Endianness?));

            return endianness;
        }

        public static Endianness? SetEndianness(this Stream stream, Endianness? endianness = null)
        {
            var previous = stream.GetEndianness();

            Endiannesses[stream] = endianness;

            return previous;
        }

        public static IDisposable SetEndiannessScope(this Stream stream, Endianness? endianness = null)
        {
            var scope = new StreamEndiannessScope(stream, endianness);

            return scope;
        }

        public static T Peek<T>(this Stream stream, Func<Stream, T> func)
        {
            var position = stream.Position;

            var value = func(stream);

            stream.Position = position;

            return value;
        }

        public static T Read<T>(this Stream stream, Endianness? endianness = null)
            where T : unmanaged
        {
            var pool = ArrayPool<byte>.Shared;

            var size = Unsafe.SizeOf<T>();

            var rent = pool.Rent(size);

            var span = rent.AsSpan(0, size);

            var read = stream.Read(span);

            if (read != size)
            {
                throw new EndOfStreamException();
            }

            if ((endianness ?? stream.GetEndianness() ?? Endianness) != Endianness)
            {
                span.Reverse();
            }

            var data = MemoryMarshal.Read<T>(rent);

            pool.Return(rent);

            return data;
        }

        public static byte[] ReadBytes(this Stream stream, int count)
        {
            var length = Math.Min(count, stream.Length - stream.Position);

            var bytes = new byte[length];

            var read = stream.Read(bytes);

            if (read != length)
            {
                throw new EndOfStreamException();
            }

            return bytes;
        }

        public static sbyte ReadSByte(this Stream stream)
        {
            var value = stream.Read<sbyte>();

            return value;
        }

        public static float ReadSingle(this Stream stream, Endianness? endianness = null)
        {
            var value = stream.Read<float>(endianness);

            return value;
        }

        public static double ReadDouble(this Stream stream, Endianness? endianness = null)
        {
            var value = stream.Read<double>(endianness);

            return value;
        }

        public static short ReadInt16(this Stream stream, Endianness? endianness = null)
        {
            var value = stream.Read<short>(endianness);

            return value;
        }

        public static int ReadInt32(this Stream stream, Endianness? endianness = null)
        {
            var value = stream.Read<int>(endianness);

            return value;
        }

        public static long ReadInt64(this Stream stream, Endianness? endianness = null)
        {
            var value = stream.Read<long>(endianness);

            return value;
        }

        public static ushort ReadUInt16(this Stream stream, Endianness? endianness = null)
        {
            var value = stream.Read<ushort>(endianness);

            return value;
        }

        public static uint ReadUInt32(this Stream stream, Endianness? endianness = null)
        {
            var value = stream.Read<uint>(endianness);

            return value;
        }

        public static ulong ReadUInt64(this Stream stream, Endianness? endianness = null)
        {
            var value = stream.Read<ulong>(endianness);

            return value;
        }

        public static void Write<T>(this Stream stream, T value, Endianness? endianness = null) where T : unmanaged
        {
            var pool = ArrayPool<byte>.Shared;

            var size = Unsafe.SizeOf<T>();

            var rent = pool.Rent(size);

            var span = rent.AsSpan(0, size);

            if ((endianness ?? stream.GetEndianness() ?? Endianness) != Endianness)
            {
                span.Reverse();
            }

            MemoryMarshal.Write(span, ref value);

            stream.Write(span);

            pool.Return(rent);
        }
    }
}