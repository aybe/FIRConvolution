using AOT;
using Unity.Burst;
using Unity.Profiling;

namespace FIRConvolution
{
    public partial struct Filter
    {
        private static readonly ProfilerMarker ProcessScalarFullMarker
            = new(ProfilerCategory.Audio, nameof(ProcessScalarFull));

        public static Filter CreateScalarFull(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 1, allocator);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethodHandler))]
        public static unsafe void ProcessScalarFull(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
            ProcessArgs(source, target, length, stride, offset, ref filter);

            using var auto = ProcessScalarFullMarker.Auto();

            var h       = filter.H;
            var z       = filter.Z;
            var hLength = filter.HLength;

            for (var sample = 0; sample < length; sample += 1)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = 0.0f;

                var tap = 0;

                for (; tap < hLength; tap += 1)
                {
                    var h0 = h[tap];
                    var z0 = z[pos - tap];

                    sum += h0 * z0;
                }

                CopyTo(sample, stride, offset, target, sum);
            }
        }
    }
}