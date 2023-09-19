using Unity.Burst;
using Unity.Mathematics;
#if FIR_PROFILE
using Unity.Profiling;
#endif

namespace FIRConvolution
{
    public partial struct Filter
    {
#if FIR_PROFILE
        private static readonly ProfilerMarker ProcessVectorFullOuterMarker
            = new(ProfilerCategory.Audio, nameof(ProcessVectorFullOuter));
#endif

        public static Filter CreateVectorFullOuter(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 4, allocator);
        }

        [BurstCompile]
        public static unsafe void ProcessVectorFullOuter(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
#if FIR_ASSERT
            ProcessArgs(source, target, length, stride, offset, ref filter);
#endif

#if FIR_PROFILE
            using var auto = ProcessVectorFullOuterMarker.Auto();
#endif

            var h       = filter.H;
            var z       = filter.Z;
            var hLength = filter.HLength;

            for (var sample = 0; sample < length; sample += 4)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = float4.zero;

                var tap = 0;

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