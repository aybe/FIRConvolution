using System.Numerics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void VectorHalfBandLoopFullOuterInner(System.Span<float> source, System.Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample <= length - v; sample += v)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 4);

                var sum = Vector4.Zero;

                int tap, len, idx;

                for (tap = filter.HOffset, len = n / (v * 2), idx = 0; idx < len; tap += v * 2, idx++)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    var zT = zGet - tap;
                
                // @formatter:off
                var z0 = z[zT - 0]; var z1 = z[zT - 1]; var z2 = z[zT - 2]; var z3 = z[zT - 3]; var z4 = z[zT - 4];
                var z5 = z[zT - 5]; var z6 = z[zT - 6]; var z7 = z[zT - 7]; var z8 = z[zT - 8]; var z9 = z[zT - 9];
                    // @formatter:on

                    sum += new Vector4(
                        h0 * z0 + h1 * z2 + h2 * z4 + h3 * z6,
                        h0 * z1 + h1 * z3 + h2 * z5 + h3 * z7,
                        h0 * z2 + h1 * z4 + h2 * z6 + h3 * z8,
                        h0 * z3 + h1 * z5 + h2 * z7 + h3 * z9);
                }

                // TODO process 4 floats 1 tap 2 tap hop

                for (; tap < n; tap += 2)
                {
                    var h0 = h[tap];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 1];
                    var z2 = z[zT - 2];
                    var z3 = z[zT - 3];

                    sum += new Vector4(h0) * new Vector4(z0, z1, z2, z3);
                }

                if (filter.TCenter)
                {
                    sum += Filter.ProcessCenterVector(ref filter);
                }

                sum.CopyTo(target[sample..]);
            }
        }
    }
}