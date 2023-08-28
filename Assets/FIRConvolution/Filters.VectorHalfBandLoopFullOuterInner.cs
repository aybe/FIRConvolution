using System;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void VectorHalfBandLoopFullOuterInner(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample <= length - v; sample += v)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 4);

                var sum = float4.zero;

                int tap, len, idx;

                for (tap = filter.HOffset, len = n / (v * 2), idx = 0; idx < len; tap += v * 2, idx++)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    var zT = zGet - tap;

                    // @formatter:off
                    var z0 = z[zT - 0]; var z1 = z[zT - 1]; var z2 = z[zT - 2]; var z3 = z[zT - 3]; var z4 = z[zT - 4];
                    var z5 = z[zT - 5]; var z6 = z[zT - 6]; var z7 = z[zT - 7]; var z8 = z[zT - 8]; var z9 = z[zT - 9];
                    // @formatter:on

                    var hv0 = new float4(h0, h1, h2, h3);
                    var zv0 = new float4(z0, z2, z4, z6);
                    var zv1 = new float4(z1, z3, z5, z7);
                    var zv2 = new float4(z2, z4, z6, z8);
                    var zv3 = new float4(z3, z5, z7, z9);

                    sum += new float4(
                        math.dot(hv0, zv0),
                        math.dot(hv0, zv1),
                        math.dot(hv0, zv2),
                        math.dot(hv0, zv3));
                }

                // TODO process 4 floats 1 tap 2 tap hop

                for (; tap < n; tap += 2)
                {
                    var h0 = h[tap];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 1];
                    var z2 = z[zT - 2];
                    var z3 = z[zT - 3];

                    var hv0 = new float4(h0);
                    var zv0 = new float4(z0, z1, z2, z3);

                    sum += hv0 * zv0;
                }

                if (filter.TCenter)
                {
                    sum += Filter.ProcessCenterVector(ref filter);
                }

                sum.CopyTo(target[sample..]);
            }
        }
    }
}