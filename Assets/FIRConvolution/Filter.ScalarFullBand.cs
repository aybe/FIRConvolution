﻿using AOT;
using Unity.Burst;
#if FIR_PROFILE_MARKERS
using Unity.Profiling;
#endif

namespace FIRConvolution
{
    public partial struct Filter
    {
#if FIR_PROFILE_MARKERS
        private static readonly ProfilerMarker FilterScalarFullMarker
            = new(ProfilerCategory.Audio, nameof(FilterScalarFullMarker));
#endif

        public static Filter CreateScalarFullBand(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 1, allocator);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethodHandler))]
        public static unsafe void ProcessScalarFullBand(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
#if FIR_CHECK_ARGS
            ProcessArgs(source, target, length, stride, offset, ref filter);
#endif

#if FIR_PROFILE_MARKERS
            using var auto = FilterScalarFullMarker.Auto();
#endif

            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample < length; sample += v)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = 0.0f;

                var tap = 0;

                for (; tap < n; tap += 1)
                {
                    var h0 = h[tap];

                    var zP = pos - tap;

                    var z0 = z[zP];

                    sum += h0 * z0;
                }

                CopyTo(sample, stride, offset, target, sum);
            }
        }
    }
}