using AOT;
using Unity.Burst;

namespace FIRConvolution
{
    public partial struct Filter
    {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void ScalarHalfBandLoopHalf(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
            ValidateArguments(source, target, length, stride, offset, ref filter);

            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;
            var c = filter.HCenter;
            var e = n - 1;

            for (var sample = 0; sample < length; sample += v)
            {
                var pos = Filter.UpdateZ(ref filter, source, sample, stride, offset);

                var sum = 0.0f;

                var tap = filter.HOffset;

                for (; tap < c; tap += 2)
                {
                    var h0 = h[tap];

                    var i0 = pos - tap;
                    var i1 = pos - (e - tap);

                    var z0 = z[i0];
                    var z1 = z[i1];

                    sum += h0 * z0 + h0 * z1;
                }

                Filter.ProcessCenterScalar(ref filter, ref sum);

                target[sample] = sum;
            }
        }
    }
}