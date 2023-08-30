using AOT;
using Unity.Burst;

namespace FIRConvolution
{
    public static partial class Filters
    {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void ScalarHalfBandLoopFull(in float* source, in float* target, in int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample < length; sample += v)
            {
                var pos = UpdateZ(ref filter, source, sample);

                var sum = 0.0f;

                var tap = filter.HOffset;

                for (; tap < n; tap += 2)
                {
                    var h0 = h[tap];

                    var zP = pos - tap;

                    var z0 = z[zP];

                    sum += h0 * z0;
                }

                if (filter.TCenter)
                {
                    sum += ProcessCenterScalar(ref filter);
                }

                target[sample] = sum;
            }
        }
    }
}