﻿using AOT;
using Unity.Burst;
using Unity.Mathematics;

namespace FIRConvolution
{
    public partial struct Filter
    {
        public static Filter CreateVectorFullBandOuter(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 4, allocator);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethod))]
        public static unsafe void ProcessVectorFullBandOuter(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
            ProcessArgs(source, target, length, stride, offset, ref filter);

            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            var k = length - v;

            for (var sample = 0; sample <= k; sample += v)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = float4.zero;

                var tap = 0;

                int end;

                for (end = n; tap < end; tap += 1)
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

                CopyTo(sum, &target[sample], stride, offset);
            }
        }
    }
}