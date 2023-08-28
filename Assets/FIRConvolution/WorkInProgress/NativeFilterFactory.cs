using System;

namespace FIRConvolution.WorkInProgress
{
    public static unsafe class NativeFilterFactory
    {
        public static NativeFilter Create(float[] h, int v)
        {
            var hLength = h.Length;

            if (hLength == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(h), h, null);
            }

            if (v < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(v), v, null);
            }

            var z = new float[(hLength + (v - 1)) * 2];

            var filter = new NativeFilter
            {
                CVector    = v,
                H          = NativeMemoryFactory.Allocate(h),
                HLength    = hLength,
                Z          = NativeMemoryFactory.Allocate(z),
                ZLength    = z.Length,
                ZOffset    = 0,
                ZOffsetGet = hLength - 1 + v - 1,
                ZOffsetSet = hLength - 0 + v - 1
            };

            return filter;
        }

        public static void Dispose(ref NativeFilter filter)
        {
            NativeMemoryFactory.Free(filter.H);
            NativeMemoryFactory.Free(filter.Z);
        }
    }
}