using AOT;
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
        private static readonly ProfilerMarker ProcessVectorHalfHalfOuterMarker
            = new(ProfilerCategory.Audio, nameof(ProcessVectorHalfHalfOuter));
#endif

        public static Filter CreateVectorHalfHalfOuter(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 4, allocator);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethodHandler))]
        public static unsafe void ProcessVectorHalfHalfOuter(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
#if FIR_ASSERT
            ProcessArgs(source, target, length, stride, offset, ref filter);
#endif

#if FIR_PROFILE
            using var auto = ProcessVectorHalfHalfOuterMarker.Auto();
#endif

            var h       = filter.H;
            var z       = filter.Z;
            var hLength = filter.HLength;
            var hCenter = filter.HCenter;
            var hOffset = filter.HOffset;

            for (var sample = 0; sample < length; sample += 4)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = float4.zero;

                var tap = hOffset;

                var idx = pos - (hLength - 1);

                for (; tap < hCenter; tap += 2)
                {
                    var h0 = h[tap];

                    var p0 = pos - tap;
                    var p1 = idx + tap;

                    var i0 = p0 - 0;
                    var i1 = p0 - 1;
                    var i2 = p0 - 2;
                    var i3 = p0 - 3;

                    var i4 = p1 - 0;
                    var i5 = p1 - 1;
                    var i6 = p1 - 2;
                    var i7 = p1 - 3;

                    var z0 = z[i0];
                    var z1 = z[i1];
                    var z2 = z[i2];
                    var z3 = z[i3];

                    var z4 = z[i4];
                    var z5 = z[i5];
                    var z6 = z[i6];
                    var z7 = z[i7];

                    var hv0 = new float4(h0, h0, h0, h0);
                    var zv0 = new float4(z0, z1, z2, z3);
                    var zv1 = new float4(z4, z5, z6, z7);

                    sum += hv0 * (zv0 + zv1);
                }

                UpdateCenterVector(ref filter, ref sum);

                CopyTo(sample, stride, offset, target, sum);
            }
        }
    }
}