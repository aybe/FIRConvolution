using System;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void ScalarHalfBandLoopHalf(Span<float> source, Span<float> target, int length, ref Filter filter)
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

                for (var tap = filter.HOffset; tap < c; tap += 2)
                {
                    var h0 = h[tap];

                    var i0 = zGet - tap;
                    var i1 = zGet - (tEnd - tap); // TODO?

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