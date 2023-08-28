using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FIRConvolution.WorkInProgress
{
    public static unsafe class NativeFilterProcess
    {
        private static void ValidateParameters(float* source, float* target, int length, int stride, int offset)
            // TODO conditional
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (length < 0 || length % 4 != 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, null);

            if (stride < 1)
                throw new ArgumentOutOfRangeException(nameof(stride), stride, null);

            if (offset < 0 || offset >= stride)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
        }

        public static void FullBandOuterInner(
            // TODO for testing, delete
            Span<float> source, Span<float> target, int length, int stride, int offset, ref NativeFilter filter)
        {
            ref var rSource = ref MemoryMarshal.GetReference(source);
            ref var rTarget = ref MemoryMarshal.GetReference(target);

            fixed (float* pSource = &rSource)
            fixed (float* pTarget = &rTarget)
            {
                FullBandOuterInner(pSource, pTarget, length, stride, offset, ref filter);
            }
        }

        public static void FullBandOuterInner(
            // TODO offset, stride
            float* source, float* target, int length, int stride, int offset, ref NativeFilter filter)
        {
            ValidateParameters(source, target, length, stride, offset);

            var cVector    = filter.CVector;
            var h          = filter.H;
            var hLength    = filter.HLength;
            var z          = filter.Z;
            var zLength    = filter.ZLength;
            var zOffsetGet = filter.ZOffsetGet;
            var zOffsetSet = filter.ZOffsetSet;

            for (var sample = 0; sample <= length - cVector; sample += cVector)
            {
                var zOffset = filter.ZOffset;

                var s0 = (sample + 0) * stride + offset;
                var s1 = (sample + 1) * stride + offset;
                var s2 = (sample + 2) * stride + offset;
                var s3 = (sample + 3) * stride + offset;

                z[zOffset + 0] = z[(zOffset + 0 + zOffsetSet) % zLength] = source[s3];
                z[zOffset + 1] = z[(zOffset + 1 + zOffsetSet) % zLength] = source[s2];
                z[zOffset + 2] = z[(zOffset + 2 + zOffsetSet) % zLength] = source[s1];
                z[zOffset + 3] = z[(zOffset + 3 + zOffsetSet) % zLength] = source[s0];

                //LogZLine();

                var sum = Vector4.Zero;

                var tap = 0;

                for (; tap < hLength - cVector; tap += cVector) // 4 overlap
                {
                    var h0Index = tap + 0;
                    var h1Index = tap + 1;
                    var h2Index = tap + 2;
                    var h3Index = tap + 3;

                    var z0Index = zOffset + zOffsetGet - tap - 0;
                    var z1Index = zOffset + zOffsetGet - tap - 1;
                    var z2Index = zOffset + zOffsetGet - tap - 2;
                    var z3Index = zOffset + zOffsetGet - tap - 3;
                    var z4Index = zOffset + zOffsetGet - tap - 4;
                    var z5Index = zOffset + zOffsetGet - tap - 5;
                    var z6Index = zOffset + zOffsetGet - tap - 6;

                    var h0 = h[h0Index];
                    var h1 = h[h1Index];
                    var h2 = h[h2Index];
                    var h3 = h[h3Index];

                    var z0 = z[z0Index];
                    var z1 = z[z1Index];
                    var z2 = z[z2Index];
                    var z3 = z[z3Index];
                    var z4 = z[z4Index];
                    var z5 = z[z5Index];
                    var z6 = z[z6Index];

                    sum += new Vector4(
                        h0 * z0 + h1 * z1 + h2 * z2 + h3 * z3,
                        h0 * z1 + h1 * z2 + h2 * z3 + h3 * z4,
                        h0 * z2 + h1 * z3 + h2 * z4 + h3 * z5,
                        h0 * z3 + h1 * z4 + h2 * z5 + h3 * z6
                    );

                    //LogLoop1(
                    //    h0Index, h1Index, h2Index, h3Index, null, null, null, null, null, null,
                    //    z0Index, z1Index, z2Index, z3Index, z4Index, z5Index, z6Index, null, null, null,
                    //    z0, z1, z2, z3, z4, z5, z6, null, null, null,
                    //    h0, h1, h2, h3, null, null, null, null, null, null);
                }

                for (; tap < hLength; tap += 1)
                {
                    var h0Index = tap + 0;

                    var z0Index = zOffset + zOffsetGet - tap - 0;
                    var z1Index = zOffset + zOffsetGet - tap - 1;
                    var z2Index = zOffset + zOffsetGet - tap - 2;
                    var z3Index = zOffset + zOffsetGet - tap - 3;

                    var h0 = h[h0Index];

                    var z0 = z[z0Index];
                    var z1 = z[z1Index];
                    var z2 = z[z2Index];
                    var z3 = z[z3Index];

                    sum += new Vector4(
                        h0 * z0,
                        h0 * z1,
                        h0 * z2,
                        h0 * z3
                    );

                    //LogLoop2(
                    //    h0Index, null, null, null, null, null, null, null, null, null,
                    //    z0Index, z1Index, z2Index, z3Index, null, null, null, null, null, null,
                    //    z0, z1, z2, z3, null, null, null, null, null, null,
                    //    h0, null, null, null, null, null, null, null, null, null);
                }

                //LogResult(sum);

                target[s0] = sum.X;
                target[s1] = sum.Y;
                target[s2] = sum.Z;
                target[s3] = sum.W;

                filter.ZOffsetUpdate();

                //LogSeparator();
            }
        }
    }
}