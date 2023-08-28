using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void VectorHalfBandLoopFullInner(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;

            for (var sample = 0; sample < length; sample += 1)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 1);

                var sum = 0.0f;

                var tap = filter.HOffset;

                int end;

                // TODO scalar 4 taps + hop

                for (end = n - 8; tap < end; tap += 8)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 2];
                    var z2 = z[zT - 4];
                    var z3 = z[zT - 6];

                    sum += h0 * z0 + h1 * z1 + h2 * z2 + h3 * z3;
                }

                for (end = n - 4; tap < end; tap += 4)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 2];

                    sum += h0 * z0 + h1 * z1;
                }

                for (end = n; tap < end; tap += 2)
                {
                    var h0 = h[tap];

                    var z0 = z[zGet - tap];

                    sum += h0 * z0;
                }

                if (filter.TCenter)
                {
                    sum += Filter.ProcessCenterScalar(ref filter);
                }

                target[sample] = sum;
            }
        }

        public static void VectorHalfBandLoopFullInnerTest(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;

            for (var sample = 0; sample < length; sample += 1)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 1);

                var sum = 0.0f;

                var tap = filter.HOffset;

                int end;
                // TODO scalar 4 taps + hop

                for (end = n - 8; tap < end; tap += 8)
                {
                    break;
                    var vh = new float4();
                    var vz = new float4();

                    var idx = new int4(0, 2, 4, 6);
                    var pos = zGet - tap;

                    Set(h, ref vh, tap + idx);
                    Set(z, ref vz, pos - idx);

                    sum += math.dot(vh, vz);
                }


                for (end = n - 4; tap < end; tap += 4)
                {
                    break;
                    var vh = new float2();
                    var vz = new float2();

                    var idx = new int2(0, 2);
                    var pos = zGet - tap;

                    Set(ref h, ref vh, tap + idx);
                    Set(ref z, ref vz, pos - idx);

                    sum += math.dot(vh, vz);
                }

                for (end = n; tap < end; tap += 2)
                {
                    break;
                    var h0 = h[tap];

                    var z0 = z[zGet - tap];

                    sum += h0 * z0;
                }

                Convolve1(ref sum, ref tap, n, 8, 4, 2, zGet, h, z); // 4 by 4
                Convolve1(ref sum, ref tap, n, 4, 2, 2, zGet, h, z); // 2 by 2
                Convolve1(ref sum, ref tap, n, 2, 2, 2, zGet, h, z); // 1 by 1

                Console.WriteLine();

                if (filter.TCenter)
                {
                    sum += Filter.ProcessCenterScalar(ref filter);
                }

                target[sample] = sum;
            }

            Console.WriteLine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Convolve1(ref float sum, ref int tap, in int len, in int hop, in int num, in int mul, in int pos, in float[] h, in float[] z)
        {
            var vh = new float4(); // coefficients

            var vz = new float4(); // delays

            var tp = new int4(0, 1, 2, 3) * mul; // taps

            var end = len - hop;

            Console.WriteLine($"tap: {tap,2}, taps: {tp}, len: {len,2}, hop: {hop,2}, num: {num,2}, mul: {mul,2}, pos: {pos,2}, end: {end,2}");

            for (; tap < end; tap += hop)
            {
                var idx = pos - tap; // Z index

                var iH = tap + tp;
                var iZ = idx - tp;

                Set(h, ref vh, iH, num);
                Set(z, ref vz, iZ, num);

                sum += math.dot(vh, vz);
            }
        }

        public static void VectorHalfBandLoopFullInnerOriginal(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;

            for (var sample = 0; sample < length; sample += 1)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 1);

                var sum = 0.0f;

                var tap = filter.HOffset;

                int end;

                // TODO scalar 4 taps + hop

                for (end = n - 8; tap < end; tap += 8)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 2];
                    var z2 = z[zT - 4];
                    var z3 = z[zT - 6];

                    sum += h0 * z0 + h1 * z1 + h2 * z2 + h3 * z3;
                }

                for (end = n - 4; tap < end; tap += 4)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 2];

                    sum += h0 * z0 + h1 * z1;
                }

                for (end = n; tap < end; tap += 2)
                {
                    var h0 = h[tap];

                    var z0 = z[zGet - tap];

                    sum += h0 * z0;
                }

                if (filter.TCenter)
                {
                    sum += Filter.ProcessCenterScalar(ref filter);
                }

                target[sample] = sum;
            }
        }
    }
}