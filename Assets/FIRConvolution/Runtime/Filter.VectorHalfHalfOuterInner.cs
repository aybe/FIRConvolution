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
        private static readonly ProfilerMarker ProcessVectorHalfHalfOuterInnerMarker
            = new(ProfilerCategory.Audio, nameof(ProcessVectorHalfHalfOuterInner));
#endif

        public static Filter CreateVectorHalfHalfOuterInner(float[] h, MemoryAllocator allocator)
        {
            return Create(h, 4, allocator);
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(FilterMethodHandler))]
        public static unsafe void ProcessVectorHalfHalfOuterInner(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
            // this algorithm was quite tricky to figure out to say the least
            // plot these values as four lines to understand where's the trap
            // first four columns are zXXA negated because of subtracting tap

            // +0, +2, +4, +6, +0, +2, +4, +6
            // +1, +3, +5, +7, -1, +1, +3, +5
            // +2, +4, +6, +8, -2, +0, +2, +4
            // +3, +5, +7, +9, -3, -1, +1, +3

            ProcessArgs(source, target, length, stride, offset, ref filter);

#if FIR_PROFILE
            using var auto = ProcessVectorHalfHalfOuterInnerMarker.Auto();
#endif

            var h       = filter.H;
            var z       = filter.Z;
            var hLength = filter.HLength;
            var hCenter = filter.HCenter;

            var szLoop1 = hCenter / 8;

            for (var sample = 0; sample < length; sample += 4)
            {
                var pos = UpdateZ(ref filter, source, sample, stride, offset);

                var sum = float4.zero;

                var tap = filter.HOffset;

                var idx = pos - (hLength - 1);

                for (var i = 0; i < szLoop1; i++, tap += 8)
                {
                    var h0 = h[tap + 0];
                    var h1 = h[tap + 2];
                    var h2 = h[tap + 4];
                    var h3 = h[tap + 6];

                    var p0 = pos - tap;
                    var p1 = idx + tap;

                    var z01A = z[p0 - 0];
                    var z02A = z[p0 - 2];
                    var z03A = z[p0 - 4];
                    var z04A = z[p0 - 6];

                    var z01B = z[p1 + 0];
                    var z02B = z[p1 + 2];
                    var z03B = z[p1 + 4];
                    var z04B = z[p1 + 6];

                    var z05A = z[p0 - 1];
                    var z06A = z[p0 - 3];
                    var z07A = z[p0 - 5];
                    var z08A = z[p0 - 7];

                    var z05B = z[p1 - 1];
                    var z06B = z[p1 + 1];
                    var z07B = z[p1 + 3];
                    var z08B = z[p1 + 5];

                    var z09A = z[p0 - 2];
                    var z10A = z[p0 - 4];
                    var z11A = z[p0 - 6];
                    var z12A = z[p0 - 8];

                    var z09B = z[p1 - 2];
                    var z10B = z[p1 + 0];
                    var z11B = z[p1 + 2];
                    var z12B = z[p1 + 4];

                    var z13A = z[p0 - 3];
                    var z14A = z[p0 - 5];
                    var z15A = z[p0 - 7];
                    var z16A = z[p0 - 9];

                    var z13B = z[p1 - 3];
                    var z14B = z[p1 - 1];
                    var z15B = z[p1 + 1];
                    var z16B = z[p1 + 3];

                    var hv0 = new float4(h0, h1, h2, h3);
                    var zv0 = new float4(z01A, z02A, z03A, z04A);
                    var zv1 = new float4(z01B, z02B, z03B, z04B);
                    var zv2 = new float4(z05A, z06A, z07A, z08A);
                    var zv3 = new float4(z05B, z06B, z07B, z08B);
                    var zv4 = new float4(z09A, z10A, z11A, z12A);
                    var zv5 = new float4(z09B, z10B, z11B, z12B);
                    var zv6 = new float4(z13A, z14A, z15A, z16A);
                    var zv7 = new float4(z13B, z14B, z15B, z16B);

                    sum += new float4(
                        math.dot(hv0, zv0 + zv1),
                        math.dot(hv0, zv2 + zv3),
                        math.dot(hv0, zv4 + zv5),
                        math.dot(hv0, zv6 + zv7));
                }

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
                    var zv1 = new float4(z0, z1, z2, z3);
                    var zv2 = new float4(z4, z5, z6, z7);

                    sum += hv0 * (zv1 + zv2);
                }

                UpdateCenterVector(ref filter, ref sum);

                CopyTo(sample, stride, offset, target, sum);
            }
        }
    }
}