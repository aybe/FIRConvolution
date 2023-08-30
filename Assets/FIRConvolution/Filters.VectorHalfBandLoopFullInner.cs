using AOT;
using Unity.Burst;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void VectorHalfBandLoopFullInner(in float* source, in float* target, in int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample < length; sample += v)
            {
                var pos = Filter.UpdateZ(ref filter, source, sample);

                var sum = 0.0f;

                var tap = filter.HOffset; // TODO?

                int end;

                for (end = n - 8; tap < end; tap += 8)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];
                    var z1 = z[zP - 2];
                    var z2 = z[zP - 4];
                    var z3 = z[zP - 6];

                    var hv = new float4(h0, h1, h2, h3);
                    var zv = new float4(z0, z1, z2, z3);

                    sum += math.dot(hv, zv);
                }

                for (end = n - 4; tap < end; tap += 4)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];
                    var z1 = z[zP - 2];

                    var hv = new float2(h0, h1);
                    var zv = new float2(z0, z1);

                    sum += math.dot(hv, zv);
                }

                for (end = n; tap < end; tap += 2)
                {
                    var h0 = h[tap];

                    var zP = pos - tap;

                    var z0 = z[zP];

                    sum += h0 * z0;
                }

                Filter.ProcessCenterScalar(ref filter, ref sum);

                target[sample] = sum;
            }
        }
    }
}