using AOT;
using Unity.Burst;
#if FIR_PROFILE
using Unity.Profiling;
#endif

namespace FIRConvolution
{
    public partial struct Filter
    {
#if FIR_PROFILE
        private static readonly ProfilerMarker ProcessScalarHalfFullMarker
            = new(ProfilerCategory.Audio, nameof(ProcessScalarHalfFull));
#endif

        public static Filter CreateScalarHalfFull(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 1, allocator);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethodHandler))]
        public static unsafe void ProcessScalarHalfFull(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
            ProcessArgs(source, target, length, stride, offset, ref filter);

#if FIR_PROFILE
            using var auto = ProcessScalarHalfFullMarker.Auto();
#endif

            var h       = filter.H;
            var z       = filter.Z;
            var hLength = filter.HLength;
            var hOffset = filter.HOffset;

            for (var sample = 0; sample < length; sample += 1)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = 0.0f;

                var tap = hOffset;

                for (; tap < hLength; tap += 2)
                {
                    var h0 = h[tap];
                    var z0 = z[pos - tap];

                    sum += h0 * z0;
                }

                UpdateCenterScalar(ref filter, ref sum);

                CopyTo(sample, stride, offset, target, sum);
            }
        }
    }
}