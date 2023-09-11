using AOT;
using Unity.Burst;
using Unity.Profiling;

namespace FIRConvolution
{
    public partial struct Filter
    {
        private static readonly ProfilerMarker FilterProfilerMarkerFilterScalarHalfHalf
            = new(ProfilerCategory.Audio, nameof(FilterProfilerMarkerFilterScalarHalfHalf));

        public static Filter CreateScalarHalfBandHalfLoop(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 1, allocator);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethodHandler))]
        public static unsafe void ProcessScalarHalfBandHalfLoop(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
            ProcessArgs(source, target, length, stride, offset, ref filter);

            using var auto = FilterProfilerMarkerFilterScalarHalfHalf.Auto();

            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;
            var c = filter.HCenter;
            var e = n - 1;

            for (var sample = 0; sample < length; sample += v)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = 0.0f;

                var tap = filter.HOffset;

                for (; tap < c; tap += 2)
                {
                    var h0 = h[tap];

                    var i0 = pos - tap;
                    var i1 = pos - (e - tap);

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