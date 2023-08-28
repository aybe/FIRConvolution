using System;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void ScalarFullBand(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;

            for (var sample = 0; sample < length; sample += 1)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 1);

                var sum = 0.0f;

                for (var tap = 0; tap < n; tap += 1)
                {
                    var h0 = h[tap];

                    var i0 = zGet - tap;

                    var z0 = z[i0];

                    sum += h0 * z0;
                }

                target[sample] = sum;
            }
        }
    }
}