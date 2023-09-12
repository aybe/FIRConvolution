using AOT;
using Unity.Burst;
using Unity.Mathematics;
#if FIR_PROFILE_MARKERS
using Unity.Profiling;
#endif

namespace FIRConvolution
{
    public partial struct Filter
    {
#if FIR_PROFILE_MARKERS
        private static readonly ProfilerMarker FilterVectorHalfFullOuterMarker
            = new(ProfilerCategory.Audio, nameof(FilterVectorHalfFullOuterMarker));
#endif

        public static Filter CreateVectorHalfBandFullLoopOuter(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 4, allocator);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethodHandler))]
        public static unsafe void ProcessVectorHalfBandFullLoopOuter(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
            ProcessArgs(source, target, length, stride, offset, ref filter);

#if FIR_PROFILE_MARKERS
            using var auto = FilterVectorHalfFullOuterMarker.Auto();
#endif
            
            var h = filter.H;
            var z = filter.Z;
            var n = filter.HLength;
            var v = filter.VLength;

            for (var sample = 0; sample < length; sample += v)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = float4.zero;

                var tap = filter.HOffset;

                for (; tap < n; tap += 2)
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

                UpdateCenterVector(ref filter, ref sum);

                CopyTo(sample, stride, offset, target, sum);
            }
        }
    }
}