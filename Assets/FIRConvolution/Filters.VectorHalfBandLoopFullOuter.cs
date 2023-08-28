using System.Numerics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void VectorHalfBandLoopFullOuter(System.Span<float> source, System.Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample < length; sample += v)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 4);

                var sum = Vector4.Zero;

                // TODO process 4 floats 1 tap 2 tap hop

                for (var tap = filter.HOffset; tap < n; tap += 2)
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