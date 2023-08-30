using AOT;
using Unity.Burst;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void VectorHalfBandLoopHalfOuter(in float* source, in float* target, in int length, ref Filter filter)
        {
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;
            var c = filter.HCenter;
            var e = n - 1;

            for (var sample = 0; sample < length; sample += v)
            {
                var pos = Filter.UpdateZ(ref filter, source, sample);

                var sum = float4.zero;

                var tap = filter.HOffset;

                for (; tap < c; tap += 2) // TODO DRY VectorHalfBandLoopHalfOuterInner
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

                    var hv0 = new float4(h0, h0, h0, h0);
                    var zv0 = new float4(z0, z1, z2, z3);
                    var zv1 = new float4(z4, z5, z6, z7);

                    sum += hv0 * (zv0 + zv1);
                }

                Filter.ProcessCenterVector(ref filter, ref sum);

                CopyTo(sum, target, sample);
            }
        }
    }
}