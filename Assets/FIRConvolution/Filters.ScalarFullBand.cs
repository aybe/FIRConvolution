using AOT;
using Unity.Burst;

namespace FIRConvolution
{
    public static partial class Filters
    {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void ScalarFullBand(
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

                for (; tap < n; tap += 1)
                {
                    var h0 = h[tap];

                    var zP = pos - tap;

                    var z0 = z[zP];

                    sum += h0 * z0;
                }

                target[sample] = sum;
            }
        }
    }
}