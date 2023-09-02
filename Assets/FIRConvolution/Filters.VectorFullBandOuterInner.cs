using AOT;
using Unity.Burst;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void VectorFullBandOuterInner(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
            ValidateArguments(source, target, length, stride, offset, ref filter);

            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            var k = length - v;

            for (var sample = 0; sample <= k; sample += v)
            {
                var pos = Filter.UpdateZ(ref filter, source, sample);

                var sum = float4.zero;

                var tap = 0;

                int end;

                for (end = n - v; tap < end; tap += v)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 1];
                    var h2 = h[tap + 2];
                    var h3 = h[tap + 3];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];
                    var z1 = z[zP - 1];
                    var z2 = z[zP - 2];
                    var z3 = z[zP - 3];
                    var z4 = z[zP - 4];
                    var z5 = z[zP - 5];
                    var z6 = z[zP - 6];

                    var hv0 = new float4(h0, h1, h2, h3);

                    var zv0 = new float4(z0, z1, z2, z3);
                    var zv1 = new float4(z1, z2, z3, z4);
                    var zv2 = new float4(z2, z3, z4, z5);
                    var zv3 = new float4(z3, z4, z5, z6);

                    sum += new float4(
                        math.dot(hv0, zv0),
                        math.dot(hv0, zv1),
                        math.dot(hv0, zv2),
                        math.dot(hv0, zv3));
                }

                for (end = n - 0; tap < end; tap += 1)
                {
                    var h0 = h[tap];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];
                    var z1 = z[zP - 1];
                    var z2 = z[zP - 2];
                    var z3 = z[zP - 3];

                    var hv = new float4(h0, h0, h0, h0);
                    var zv = new float4(z0, z1, z2, z3);

                    sum += hv * zv;
                }

                CopyTo(sum, target, sample);
            }
        }
    }
}