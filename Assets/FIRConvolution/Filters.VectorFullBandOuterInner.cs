using System;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void VectorFullBandOuterInner(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample <= length - v; sample += v)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 4);

                var sum = float4.zero;

                var tap = 0;

                for (; tap < n - v; tap += v)
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
                    var z4 = z[zT - 4];
                    var z5 = z[zT - 5];
                    var z6 = z[zT - 6];

                    var hv0 = new float4(h0, h1, h2, h3);

                    var zv0 = new float4(z0, z1, z2, z3);
                    var zv1 = new float4(z1, z2, z3, z4);
                    var zv2 = new float4(z2, z3, z4, z5);
                    var zv3 = new float4(z3, z4, z5, z6);

                    sum += new float4(
                        math.dot(hv0, zv0),
                        math.dot(hv0, zv1),
                        math.dot(hv0, zv2),
                        math.dot(hv0, zv3));
                }

                // TODO process 4 floats 1 tap 1 tap hop

                for (; tap < n; tap += 1)
                {
                    var h0 = h[tap];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 1];
                    var z2 = z[zT - 2];
                    var z3 = z[zT - 3];

                    sum += new float4(h0) * new float4(z0, z1, z2, z3);
                }

                sum.CopyTo(target[sample..]);
            }
        }
    }
}