using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using FIRConvolution.Tests.Extensions;

namespace FIRConvolution.Tests.Formats.Audio.Microsoft
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4, CharSet = CharSet.Ansi)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public readonly struct FourCC
    {
        public FourCC(Stream stream)
        {
            var value = stream.ReadUInt32(Endianness.LittleEndian);

            Value = value;
        }

        public FourCC(string name)
        {
            if (name.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }

            if (name.Any(s => s > '\u007F'))
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }

            var chars = name.ToCharArray();
            var bytes = Encoding.ASCII.GetBytes(chars);
            var value = BitConverter.ToUInt32(bytes);

            Value = value;
        }

        private uint Value { get; }

        public override string ToString()
        {
            var bytes = BitConverter.GetBytes(Value);
            var value = Encoding.ASCII.GetString(bytes);

            return value;
        }

        public uint ToUInt32()
        {
            return Value;
        }

        public static implicit operator FourCC(string value)
        {
            return new FourCC(value);
        }

        public static implicit operator string(FourCC value)
        {
            return value.ToString();
        }
    }
}