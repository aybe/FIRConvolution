using AOT;
using Unity.Burst;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void VectorHalfBandLoopHalfInner(float* source, float* target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var c = filter.HCenter;
            var v = filter.VLength;
            var e = n - 1;

            for (var sample = 0; sample < length; sample += v)
            {
                var pos = Filter.UpdateZ(ref filter, source, sample);

                var sum = 0.0f;

                var tap = filter.HOffset;

                int end;

                for (end = c - 8; tap < end; tap += 8)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    var i0 = pos - (tap + 0);
                    var i1 = pos - (tap + 2);
                    var i2 = pos - (tap + 4);
                    var i3 = pos - (tap + 6);

                    var i4 = pos - (e - (tap + 0));
                    var i5 = pos - (e - (tap + 2));
                    var i6 = pos - (e - (tap + 4));
                    var i7 = pos - (e - (tap + 6));

                    var z0 = z[i0];
                    var z1 = z[i1];
                    var z2 = z[i2];
                    var z3 = z[i3];

                    var z4 = z[i4];
                    var z5 = z[i5];
                    var z6 = z[i6];
                    var z7 = z[i7];

                    var hv0 = new float4(h0, h1, h2, h3);
                    var zv0 = new float4(z0, z1, z2, z3);
                    var zv1 = new float4(z4, z5, z6, z7);

                    sum += math.dot(hv0, zv0 + zv1);
                }

                for (end = c - 2; tap < end; tap += 4)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];

                    var i0 = pos - (tap + 0);
                    var i1 = pos - (tap + 2);

                    var i2 = pos - (e - (tap + 0));
                    var i3 = pos - (e - (tap + 2));

                    var z0 = z[i0];
                    var z1 = z[i1];

                    var z2 = z[i2];
                    var z3 = z[i3];

                    var hv0 = new float2(h0, h1);
                    var zv0 = new float2(z0, z1);
                    var zv1 = new float2(z2, z3);

                    sum += math.dot(hv0, zv0 + zv1);
                }

                for (end = c - 0; tap < end; tap += 2)
                {
                    var h0 = h[tap];

                    var i0 = pos - tap;
                    var i1 = pos - (e - tap);

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