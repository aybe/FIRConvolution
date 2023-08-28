using System;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void ScalarHalfBandLoopFull(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;

            for (var sample = 0; sample < length; sample += 1)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 1);

                var sum = 0.0f;

                for (var tap = filter.HOffset; tap < n; tap += 2)
                {
                    var h0 = h[tap];

                    var z0 = z[zGet - tap];

                    sum += h0 * z0;
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