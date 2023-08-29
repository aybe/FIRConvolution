using System;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static unsafe void VectorHalfBandLoopFullOuterInner(float* source, float* target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            var k = length - v;

            for (var sample = 0; sample <= k; sample += v)
            {
                var pos = Filter.UpdateZ(ref filter, source, sample);

                var sum = float4.zero;

                var tap = filter.HOffset;

                var idx = 0;

                int end;

                for (end = n / (v * 2); idx < end; tap += v * 2, idx++)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];
                    var z1 = z[zP - 1];
                    var z2 = z[zP - 2];
                    var z3 = z[zP - 3];
                    var z4 = z[zP - 4];
                    var z5 = z[zP - 5];
                    var z6 = z[zP - 6];
                    var z7 = z[zP - 7];
                    var z8 = z[zP - 8];
                    var z9 = z[zP - 9];

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

                for (end = n; tap < end; tap += 2)
                {
                    var h0 = h[tap];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];
                    var z1 = z[zP - 1];
                    var z2 = z[zP - 2];
                    var z3 = z[zP - 3];

                    var hv = new float4(h0, h0, h0, h0);
                    var zv = new float4(z0, z1, z2, z3);

                    sum += hv * zv;
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