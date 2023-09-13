using System;

namespace FIRConvolution
{
    internal sealed class SpuReverbBuffer<T>
    {
        public SpuReverbBuffer(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Items = new T[length];
        }

        private int Index { get; set; }

        private T[] Items { get; }

        public ref T this[int index]
        {
            get
            {
                var n = Index + index;
                var m = Items.Length;
                var i = (n % m + m) % m;

                return ref Items[i];
            }
        }

        public void Advance(int count = 2)
        {
            Index = (Index + count) % Items.Length;
        }
    }
}