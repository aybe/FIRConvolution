﻿using Unity.Burst;
using Unity.Mathematics;
#if FIR_PROFILE
using Unity.Profiling;
#endif

namespace FIRConvolution
{
    public partial struct Filter
    {
#if FIR_PROFILE
        private static readonly ProfilerMarker ProcessVectorFullOuterInnerMarker
            = new(ProfilerCategory.Audio, nameof(ProcessVectorFullOuterInner));
#endif

        public static Filter CreateVectorFullOuterInner(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 4, allocator);
        }

        [BurstCompile]
        public static unsafe void ProcessVectorFullOuterInner(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
#if FIR_ASSERT
            ProcessArgs(source, target, length, stride, offset, ref filter);
#endif

#if FIR_PROFILE
            using var auto = ProcessVectorFullOuterInnerMarker.Auto();
#endif

            var h       = filter.H;
            var z       = filter.Z;
            var hLength = filter.HLength;

            var szLoop1 = hLength - 4;

            for (var sample = 0; sample < length; sample += 4)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = float4.zero;

                var tap = 0;

                for (; tap < szLoop1; tap += 4)
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

                for (; tap < hLength; tap += 1)
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

                CopyTo(sample, stride, offset, target, sum);
            }
        }
    }
}