using AOT;
using Unity.Burst;

namespace FIRConvolution
{
    public partial struct Filter
    {
        public static Filter CreateScalarHalfBandFullLoop(float[] h)
        {
            return Create(h, 1);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void ProcessScalarHalfBandFullLoop(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
            ValidateArguments(source, target, length, stride, offset, ref filter);

            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample < length; sample += v)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = 0.0f;

                var tap = filter.HOffset;

                for (; tap < n; tap += 2)
                {
                    var h0 = h[tap];

                    var zP = pos - tap;

                    var z0 = z[zP];

                    sum += h0 * z0;
                }

                UpdateCenterScalar(ref filter, ref sum);

                target[sample] = sum;
            }
        }
    }
}