using System.Numerics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void VectorFullBandOuterInner(System.Span<float> source, System.Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample <= length - v; sample += v)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 4);

                var sum = Vector4.Zero;

                var tap = 0;

                for (; tap < n - v; tap += v)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 1];
                    var h2 = h[tap + 2];
                    var h3 = h[tap + 3];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 1];
                    var z2 = z[zT - 2];
                    var z3 = z[zT - 3];
                    var z4 = z[zT - 4];
                    var z5 = z[zT - 5];
                    var z6 = z[zT - 6];

                    sum += new Vector4(
                        h0 * z0 + h1 * z1 + h2 * z2 + h3 * z3,
                        h0 * z1 + h1 * z2 + h2 * z3 + h3 * z4,
                        h0 * z2 + h1 * z3 + h2 * z4 + h3 * z5,
                        h0 * z3 + h1 * z4 + h2 * z5 + h3 * z6);
                }

                // TODO process 4 floats 1 tap 1 tap hop

                for (; tap < n; tap += 1)
                {
                    var h0 = h[tap];

                    var zT = zGet - tap;

                    var z0 = z[zT - 0];
                    var z1 = z[zT - 1];
                    var z2 = z[zT - 2];
                    var z3 = z[zT - 3];

                    sum += new Vector4(h0) * new Vector4(z0, z1, z2, z3);
                }

                sum.CopyTo(target[sample..]);
            }
        }
    }
}