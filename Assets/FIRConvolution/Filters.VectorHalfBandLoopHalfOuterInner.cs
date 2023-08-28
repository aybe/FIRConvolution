using System;
using System.Numerics;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        public static void VectorHalfBandLoopHalfOuterInner(Span<float> source, Span<float> target, int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;
            var c = filter.HCenter;

            var tEnd = n - 1; // TODO extract tEnd

            for (var sample = 0; sample <= length - v; sample += v)
            {
                var zGet = Filter.UpdateZ(ref filter, source, sample, 4);

                var sum = float4.zero;

                int tap, len, idx;

                for (tap = filter.HOffset, len = c / (v * 2), idx = 0; idx < len; tap += v * 2, idx++)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    // TODO these additions could be vectorized as well

                    // @formatter:off
                    var i01A = zGet - (tap + 0); var i01B = zGet - (tEnd - (tap + 0));
                    var i02A = zGet - (tap + 2); var i02B = zGet - (tEnd - (tap + 2));
                    var i03A = zGet - (tap + 4); var i03B = zGet - (tEnd - (tap + 4));
                    var i04A = zGet - (tap + 6); var i04B = zGet - (tEnd - (tap + 6));

                    var i05A = zGet - (tap + 1); var i05B = zGet - (tEnd - (tap - 1));
                    var i06A = zGet - (tap + 3); var i06B = zGet - (tEnd - (tap + 1));
                    var i07A = zGet - (tap + 5); var i07B = zGet - (tEnd - (tap + 3));
                    var i08A = zGet - (tap + 7); var i08B = zGet - (tEnd - (tap + 5));

                    var i09A = zGet - (tap + 2); var i09B = zGet - (tEnd - (tap - 2));
                    var i10A = zGet - (tap + 4); var i10B = zGet - (tEnd - (tap + 0));
                    var i11A = zGet - (tap + 6); var i11B = zGet - (tEnd - (tap + 2));
                    var i12A = zGet - (tap + 8); var i12B = zGet - (tEnd - (tap + 4));

                    var i13A = zGet - (tap + 3); var i13B = zGet - (tEnd - (tap - 3));
                    var i14A = zGet - (tap + 5); var i14B = zGet - (tEnd - (tap - 1));
                    var i15A = zGet - (tap + 7); var i15B = zGet - (tEnd - (tap + 1));
                    var i16A = zGet - (tap + 9); var i16B = zGet - (tEnd - (tap + 3));

                    var z01A = z[i01A]; var z01B = z[i01B];
                    var z02A = z[i02A]; var z02B = z[i02B];
                    var z03A = z[i03A]; var z03B = z[i03B];
                    var z04A = z[i04A]; var z04B = z[i04B];

                    var z05A = z[i05A]; var z05B = z[i05B];
                    var z06A = z[i06A]; var z06B = z[i06B];
                    var z07A = z[i07A]; var z07B = z[i07B];
                    var z08A = z[i08A]; var z08B = z[i08B];

                    var z09A = z[i09A]; var z09B = z[i09B];
                    var z10A = z[i10A]; var z10B = z[i10B];
                    var z11A = z[i11A]; var z11B = z[i11B];
                    var z12A = z[i12A]; var z12B = z[i12B];

                    var z13A = z[i13A]; var z13B = z[i13B];
                    var z14A = z[i14A]; var z14B = z[i14B];
                    var z15A = z[i15A]; var z15B = z[i15B];
                    var z16A = z[i16A]; var z16B = z[i16B];
                    // @formatter:on

                    var hv0 = new float4(h0, h1, h2, h3);

                    var zv0 = new float4(z01A, z02A, z03A, z04A);
                    var zv1 = new float4(z01B, z02B, z03B, z04B);

                    var zv2 = new float4(z05A, z06A, z07A, z08A);
                    var zv3 = new float4(z05B, z06B, z07B, z08B);

                    var zv4 = new float4(z09A, z10A, z11A, z12A);
                    var zv5 = new float4(z09B, z10B, z11B, z12B);

                    var zv6 = new float4(z13A, z14A, z15A, z16A);
                    var zv7 = new float4(z13B, z14B, z15B, z16B);

                    sum += new float4(
                        math.dot(hv0, zv0 + zv1),
                        math.dot(hv0, zv2 + zv3),
                        math.dot(hv0, zv4 + zv5),
                        math.dot(hv0, zv6 + zv7));
                }

                // TODO process 4 floats 1 tap 1 tap hop symmetrical

                for (; tap < c; tap += 2) // TODO DRY VectorHalfBandLoopHalfOuter
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

                    var hv0 = new float4(h0);
                    var zv1 = new float4(z0, z1, z2, z3);
                    var zv2 = new float4(z4, z5, z6, z7);
                    sum += hv0 * (zv1 + zv2);
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