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
        private static readonly ProfilerMarker ProcessVectorHalfHalfInnerMarker
            = new(ProfilerCategory.Audio, nameof(ProcessVectorHalfHalfInner));
#endif

        public static Filter CreateVectorHalfHalfInner(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 1, allocator);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethodHandler))]
        public static unsafe void ProcessVectorHalfHalfInner(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
#if FIR_ASSERT
            ProcessArgs(source, target, length, stride, offset, ref filter);
#endif

#if FIR_PROFILE
            using var auto = ProcessVectorHalfHalfInnerMarker.Auto();
#endif

            var h       = filter.H;
            var z       = filter.Z;
            var hLength = filter.HLength - 1;
            var hCenter = filter.HCenter;

            var szLoop1 = hCenter - 8;
            var szLoop2 = hCenter - 2;

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

                    var i0 = pos - (tap + 0);
                    var i1 = pos - (tap + 2);
                    var i2 = pos - (tap + 4);
                    var i3 = pos - (tap + 6);

                    var i4 = pos - (hLength - (tap + 0));
                    var i5 = pos - (hLength - (tap + 2));
                    var i6 = pos - (hLength - (tap + 4));
                    var i7 = pos - (hLength - (tap + 6));

                    var z0 = z[i0];
                    var z1 = z[i1];
                    var z2 = z[i2];
                    var z3 = z[i3];

                    var z4 = z[i4];
                    var z5 = z[i5];
                    var z6 = z[i6];
                    var z7 = z[i7];

                    var hv0 = new float4(h0, h1, h2, h3);
                    var zv0 = new float4(z0, z1, z2, z3);
                    var zv1 = new float4(z4, z5, z6, z7);

                    sum += math.dot(hv0, zv0 + zv1);
                }

                for (; tap < szLoop2; tap += 4)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];

                    var i0 = pos - (tap + 0);
                    var i1 = pos - (tap + 2);

                    var i2 = pos - (hLength - (tap + 0));
                    var i3 = pos - (hLength - (tap + 2));

                    var z0 = z[i0];
                    var z1 = z[i1];

                    var z2 = z[i2];
                    var z3 = z[i3];

                    var hv0 = new float2(h0, h1);
                    var zv0 = new float2(z0, z1);
                    var zv1 = new float2(z2, z3);

                    sum += math.dot(hv0, zv0 + zv1);
                }

                for (; tap < hCenter; tap += 2)
                {
                    var h0 = h[tap];

                    var i0 = pos - tap;
                    var i1 = pos - (hLength - tap);

                    var z0 = z[i0];
                    var z1 = z[i1];

                    sum += h0 * (z0 + z1);
                }

                UpdateCenterScalar(ref filter, ref sum);

                CopyTo(sample, stride, offset, target, sum);
            }
        }
    }
}