using AOT;
using Unity.Burst;

#if FIR_NEW

#else
using Unity.Mathematics;
#endif

namespace FIRConvolution
{
    public static partial class Filters
    {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void VectorFullBandInner(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
            ValidateArguments(source, target, length, stride, offset, ref filter);

            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample < length; sample += v)
            {
                var pos = Filter.UpdateZ(ref filter, source, sample);

                var sum = 0.0f;

                var tap = 0;

#if FIR_NEW
                Convolve1(ref sum, ref tap, n, 4, 1, 4, 4, pos, h, z);
#else
                int end;

                for (end = n - 4; tap < end; tap += 4)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 1];
                    var h2 = h[tap + 2];
                    var h3 = h[tap + 3];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];
                    var z1 = z[zP - 1];
                    var z2 = z[zP - 2];
                    var z3 = z[zP - 3];

                    var hv = new float4(h0, h1, h2, h3);
                    var zh = new float4(z0, z1, z2, z3);

                    sum += math.dot(hv, zh);
                }
#endif

#if FIR_NEW
                Convolve1(ref sum, ref tap, n, 2, 1, 2, 2, pos, h, z);
#else
                for (end = n - 1; tap < end; tap += 2)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 1];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];
                    var z1 = z[zP - 1];

                    var hv = new float2(h0, h1);
                    var zv = new float2(z0, z1);

                    sum += math.dot(hv, zv);
                }
#endif

#if FIR_NEW
                Convolve1(ref sum, ref tap, n, 1, 1, 1, 1, pos, h, z);
#else
                for (end = n - 0; tap < end; tap += 1)
                {
                    var h0 = h[tap];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];

                    sum += math.dot(h0, z0);
                }
#endif

                target[sample] = sum;
            }
        }
    }
}