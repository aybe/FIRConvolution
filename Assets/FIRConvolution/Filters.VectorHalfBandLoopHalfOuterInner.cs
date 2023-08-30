using AOT;
using Unity.Burst;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void VectorHalfBandLoopHalfOuterInner(in float* source, in float* target, in int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;
            var c = filter.HCenter;
            var d = length - v;
            var e = n - 1;
            var f = v * 2;

            for (var sample = 0; sample <= d; sample += v)
            {
                var pos = Filter.UpdateZ(ref filter, source, sample);

                var sum = float4.zero;

                var tap = filter.HOffset;

                var idx = 0;

                int len;

                for (len = c / f; idx < len; tap += f, idx++)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    // @formatter:off
                    var i01A = pos - (tap + 0); var i01B = pos - (e - (tap + 0));
                    var i02A = pos - (tap + 2); var i02B = pos - (e - (tap + 2));
                    var i03A = pos - (tap + 4); var i03B = pos - (e - (tap + 4));
                    var i04A = pos - (tap + 6); var i04B = pos - (e - (tap + 6));

                    var i05A = pos - (tap + 1); var i05B = pos - (e - (tap - 1));
                    var i06A = pos - (tap + 3); var i06B = pos - (e - (tap + 1));
                    var i07A = pos - (tap + 5); var i07B = pos - (e - (tap + 3));
                    var i08A = pos - (tap + 7); var i08B = pos - (e - (tap + 5));

                    var i09A = pos - (tap + 2); var i09B = pos - (e - (tap - 2));
                    var i10A = pos - (tap + 4); var i10B = pos - (e - (tap + 0));
                    var i11A = pos - (tap + 6); var i11B = pos - (e - (tap + 2));
                    var i12A = pos - (tap + 8); var i12B = pos - (e - (tap + 4));

                    var i13A = pos - (tap + 3); var i13B = pos - (e - (tap - 3));
                    var i14A = pos - (tap + 5); var i14B = pos - (e - (tap - 1));
                    var i15A = pos - (tap + 7); var i15B = pos - (e - (tap + 1));
                    var i16A = pos - (tap + 9); var i16B = pos - (e - (tap + 3));

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

                for (; tap < c; tap += 2)
                {
                    var h0 = h[tap];

                    var i0 = pos - tap - 0;
                    var i1 = pos - tap - 1;
                    var i2 = pos - tap - 2;
                    var i3 = pos - tap - 3;

                    var i4 = pos - e + tap - 0;
                    var i5 = pos - e + tap - 1;
                    var i6 = pos - e + tap - 2;
                    var i7 = pos - e + tap - 3;

                    var z0 = z[i0];
                    var z1 = z[i1];
                    var z2 = z[i2];
                    var z3 = z[i3];

                    var z4 = z[i4];
                    var z5 = z[i5];
                    var z6 = z[i6];
                    var z7 = z[i7];

                    var hv0 = new float4(h0);
                    var zv1 = new float4(z0, z1, z2, z3);
                    var zv2 = new float4(z4, z5, z6, z7);

                    sum += hv0 * (zv1 + zv2);
                }

                sum += filter.TCenter * Filter.ProcessCenterVector(ref filter);

                CopyTo(sum, target, sample);
            }
        }
    }
}