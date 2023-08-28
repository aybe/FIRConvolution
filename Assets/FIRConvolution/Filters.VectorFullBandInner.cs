using System;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void VectorFullBandInner(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;

            for (var sample = 0; sample < length; sample += 1)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 1);

                var sum = 0.0f;

                var tap = 0;

                // TODO scalar 4 taps + hop

                for (; tap < n - 3; tap += 4)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 1];
                    var h2 = h[tap + 2];
                    var h3 = h[tap + 3];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 1];
                    var z2 = z[zT - 2];
                    var z3 = z[zT - 3];

                    sum += h0 * z0 + h1 * z1 + h2 * z2 + h3 * z3;
                }

                // TODO this could be further vectorized

                for (; tap < n - 1; tap += 2)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 1];
                    var z0 = z[zGet - (tap + 0)];
                    var z1 = z[zGet - (tap + 1)];

                    sum += math.dot(new float2(h0, h1), new float2(z0, z1)); // todo
                }


                for (; tap < n; tap += 1)
                {
                    var h0 = h[tap];

                    var z0 = z[zGet - tap];

                    sum += h0 * z0;
                }

                target[sample] = sum;
            }
        }
    }

    public static partial class Filters
    {
        public static void VectorFullBandInnerTest(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;

            for (var sample = 0; sample < length; sample += 1)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 1);

                var sum = 0.0f;

                var tap = 0;

                // TODO scalar 4 taps + hop

                for (; tap < n - 3; tap += 4)
                {
                    // break;
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 1];
                    var h2 = h[tap + 2];
                    var h3 = h[tap + 3];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 1];
                    var z2 = z[zT - 2];
                    var z3 = z[zT - 3];

                    sum += h0 * z0 + h1 * z1 + h2 * z2 + h3 * z3;

                    continue;

                    var vh = new float4();
                    var vz = new float4();
                    var v1 = new int4();
                    var v0 = new int4();
                    var v2 = new int4();

                    v0.Set(0, 1, 2, 3);
                    v1.Set(tap);
                    v2.Set(zT);

                    Set(h, ref vh, v1 + v0);
                    Set(z, ref vz, v2 - v0);

                    sum += math.dot(vh, vz);
                }


                // TODO this could be further vectorized

                for (; tap < n - 1; tap += 2)
                {
                    //   break;
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 1];
                    var z0 = z[zGet - (tap + 0)];
                    var z1 = z[zGet - (tap + 1)];

                    sum += math.dot(new float2(h0, h1), new float2(z0, z1)); // todo
                }


                for (; tap < n; tap += 1)
                {
                    //  break;
                    var h0 = h[tap];

                    var z0 = z[zGet - tap];

                    sum += h0 * z0;
                }

                // BUG when we use this, the 1st value is always 0, find why
                Convolve1(ref sum, ref tap, n, 4, 4, 1, zGet, h, z); // 4 by 4
                Convolve1(ref sum, ref tap, n, 2, 2, 1, zGet, h, z); // 2 by 2
                Convolve1(ref sum, ref tap, n, 1, 1, 1, zGet, h, z); // 1 by 1

                target[sample] = sum;
            }
        }
    }

    public static partial class Filters
    {
        public static void VectorFullBandInnerVec(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;

            var hv = new float4();
            var zv = new float4();
            var i0 = new int4();
            var i1 = new int4();
            var i2 = new int4();

            i2.Set(0, 1, 2, 3);

            for (var sample = 0; sample < length; sample += 1)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 1);

                var sum = 0.0f;

                var tap = 0;

                // TODO scalar 4 taps + hop

                for (; tap < n - 4; tap += 4)
                {
                    i0.Set(tap);
                    i1.Set(zGet - tap);
                    Set(h, ref hv, i0 + i2);
                    Set(z, ref zv, i1 - i2);
                    sum += math.dot(hv, zv);
                }

                for (; tap < n; tap += 1)
                {
                    var h0 = h[tap];

                    var z0 = z[zGet - tap];

                    sum += h0 * z0;
                }

                target[sample] = sum;
            }
        }
    }
}