using System.Numerics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void VectorHalfBandLoopHalfOuter(System.Span<float> source, System.Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;
            var c = filter.HCenter;

            var tEnd = n - 1; // TODO extract tEnd

            for (var sample = 0; sample < length; sample += v)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 4);

                var sum = Vector4.Zero;

                var tap = filter.HOffset;

                // TODO process 4 floats 1 tap 1 tap hop symmetrical

                for (; tap < c; tap += 2) // TODO DRY VectorHalfBandLoopHalfOuterInner
                {
                    var h0 = h[tap];

                // @formatter:off
                var i0 = zGet - tap - 0; var i4 = zGet - tEnd + tap - 0;
                var i1 = zGet - tap - 1; var i5 = zGet - tEnd + tap - 1;
                var i2 = zGet - tap - 2; var i6 = zGet - tEnd + tap - 2;
                var i3 = zGet - tap - 3; var i7 = zGet - tEnd + tap - 3;

                var z0 = z[i0]; var z4 = z[i4];
                var z1 = z[i1]; var z5 = z[i5];
                var z2 = z[i2]; var z6 = z[i6];
                var z3 = z[i3]; var z7 = z[i7];
                    // @formatter:on

                    sum += new Vector4(h0 * (z0 + z4), h0 * (z1 + z5), h0 * (z2 + z6), h0 * (z3 + z7));
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