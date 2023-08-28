using System;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void VectorHalfBandLoopHalfInner(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var c = filter.HCenter;

            var tEnd = n - 1; // TODO extract tEnd

            for (var sample = 0; sample < length; sample += 1)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 1);

                var sum = 0.0f;

                var tap = filter.HOffset;

                int end;

                for (end = c - 8; tap < end; tap += 8)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    // @formatter:off
                    var i0 = zGet - (tap + 0); var i4 = zGet - (tEnd - (tap + 0));
                    var i1 = zGet - (tap + 2); var i5 = zGet - (tEnd - (tap + 2));
                    var i2 = zGet - (tap + 4); var i6 = zGet - (tEnd - (tap + 4));
                    var i3 = zGet - (tap + 6); var i7 = zGet - (tEnd - (tap + 6));

                    var z0 = z[i0]; var z4 = z[i4];
                    var z1 = z[i1]; var z5 = z[i5];
                    var z2 = z[i2]; var z6 = z[i6];
                    var z3 = z[i3]; var z7 = z[i7];
                    // @formatter:on

                    var hv0 = new float4(h0, h1, h2, h3);
                    var zv0 = new float4(z0, z1, z2, z3);
                    var zv1 = new float4(z4, z5, z6, z7);

                    sum += math.dot(hv0, zv0 + zv1);
                }

                for (end = c - 2; tap < end; tap += 4)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];

                    // @formatter:off
                    var i0 = zGet - (tap + 0); var i2 = zGet - (tEnd - (tap + 0));
                    var i1 = zGet - (tap + 2); var i3 = zGet - (tEnd - (tap + 2));

                    var z0 = z[i0]; var z2 = z[i2];
                    var z1 = z[i1]; var z3 = z[i3];
                    // @formatter:on

                    var hv0 = new float2(h0, h1);
                    var zv0 = new float2(z0, z1);
                    var zv1 = new float2(z2, z3);

                    sum += math.dot(hv0, zv0 + zv1);
                }

                for (end = c - 0; tap < end; tap += 2)
                {
                    var h0 = h[tap + 0];

                    var i0 = zGet - (tap + 0);
                    var i1 = zGet - (tEnd - (tap + 0));

                    var z0 = z[i0];
                    var z1 = z[i1];

                    sum += h0 * (z0 + z1);
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