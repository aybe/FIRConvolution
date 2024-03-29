﻿using Unity.Burst;
#if FIR_PROFILE
using Unity.Profiling;
#endif

namespace FIRConvolution
{
    public partial struct Filter
    {
#if FIR_PROFILE
        private static readonly ProfilerMarker ProcessScalarHalfHalfMarker
            = new(ProfilerCategory.Audio, nameof(ProcessScalarHalfHalf));
#endif

        public static Filter CreateScalarHalfHalf(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 1, allocator);
        }

        [BurstCompile]
        public static unsafe void ProcessScalarHalfHalf(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
#if FIR_ASSERT
            ProcessArgs(source, target, length, stride, offset, ref filter);
#endif

#if FIR_PROFILE
            using var auto = ProcessScalarHalfHalfMarker.Auto();
#endif

            var h       = filter.H;
            var z       = filter.Z;
            var hLength = filter.HLength;
            var hCenter = filter.HCenter;
            var hOffset = filter.HOffset;

            for (var sample = 0; sample < length; sample += 1)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = 0.0f;

                var tap = hOffset;

                var idx = pos - (hLength - 1);

                for (; tap < hCenter; tap += 2)
                {
                    var h0 = h[tap];

                    var i0 = pos - tap;
                    var i1 = idx + tap;

                    var z0 = z[i0];
                    var z1 = z[i1];

                    sum += h0 * z0 + h0 * z1;
                }

                UpdateCenterScalar(ref filter, ref sum);

                CopyTo(sample, stride, offset, target, sum);
            }
        }
    }
}