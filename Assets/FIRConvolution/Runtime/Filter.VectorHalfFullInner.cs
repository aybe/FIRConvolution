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
        private static readonly ProfilerMarker ProcessVectorHalfFullInnerMarker
            = new(ProfilerCategory.Audio, nameof(ProcessVectorHalfFullInner));
#endif

        public static Filter CreateVectorHalfFullInner(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 1, allocator);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethodHandler))]
        public static unsafe void ProcessVectorHalfFullInner(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
#if FIR_ASSERT
            ProcessArgs(source, target, length, stride, offset, ref filter);
#endif

#if FIR_PROFILE
            using var auto = ProcessVectorHalfFullInnerMarker.Auto();
#endif

            var h       = filter.H;
            var z       = filter.Z;
            var hLength = filter.HLength;

            var szLoop1 = hLength - 8;
            var szLoop2 = hLength - 4;
            var szLoop3 = hLength - 0;

            for (var sample = 0; sample < length; sample += 1)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = 0.0f;

                var tap = filter.HOffset;

                for (; tap < szLoop1; tap += 8)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];
                    var z1 = z[zP - 2];
                    var z2 = z[zP - 4];
                    var z3 = z[zP - 6];

                    var hv = new float4(h0, h1, h2, h3);
                    var zv = new float4(z0, z1, z2, z3);

                    sum += math.dot(hv, zv);
                }

                for (; tap < szLoop2; tap += 4)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];

                    var zP = pos - tap;

                    var z0 = z[zP - 0];
                    var z1 = z[zP - 2];

                    var hv = new float2(h0, h1);
                    var zv = new float2(z0, z1);

                    sum += math.dot(hv, zv);
                }

                for (; tap < szLoop3; tap += 2)
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