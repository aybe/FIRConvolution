﻿using System;
using System.Linq;
using Unity.Burst;
using Unity.Mathematics;
#if FIR_PROFILE // ~30% slower when enabled
using Unity.Profiling;
#endif

namespace FIRConvolution
{
    [BurstCompile]
    public unsafe partial struct Filter
    {
        private Filter(float[] h, int hOffset, int vLength, MemoryAllocator allocator)
        {
            var hLength = h.Length;

            if (hLength % 2 is not 1)
            {
                throw new ArgumentOutOfRangeException(nameof(h),
                    "The number of coefficients must be odd.");
            }

            if (hOffset is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(hOffset),
                    "The index of the first coefficient must be 0 or 1.");
            }

            if (vLength is < 1 or > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(vLength),
                    "The vectorization count must be between 1 and 4.");
            }

            if (allocator == null)
            {
                throw new ArgumentNullException(nameof(allocator));
            }

            var hCenter = hLength / 2;

            var zLength = (hLength + (vLength - 1)) * 2;

            var z = new float[zLength];

            H          = (float*)allocator.AlignedAlloc(h);
            HCenter    = hCenter;
            HLength    = hLength;
            HMiddle    = hCenter % 2 == 1 || hOffset == 1 ? 1.0f : 0.0f;
            HOffset    = hOffset;
            VLength    = vLength;
            Z          = (float*)allocator.AlignedAlloc(z);
            ZLength    = zLength;
            ZOffset    = vLength; // see comment in UpdateZ
            ZOffsetGet = 0;
            ZOffsetSet = hLength + vLength - 1;
        }

        /// <summary>
        ///     The taps.
        /// </summary>
        private readonly float* H;

        /// <summary>
        ///     The index of the center tap.
        /// </summary>
        private readonly int HCenter;

        /// <summary>
        ///     The number of taps.
        /// </summary>
        private readonly int HLength;

        /// <summary>
        ///     The value of the center tap.
        /// </summary>
        private readonly float HMiddle;

        /// <summary>
        ///     The index of the first tap.
        /// </summary>
        private readonly int HOffset;

        /// <summary>
        ///     The vectorization count.
        /// </summary>
        private readonly int VLength;

        /// <summary>
        ///     The delay line (doubled).
        /// </summary>
        private readonly float* Z;

        /// <summary>
        ///     The delay line length.
        /// </summary>
        private readonly int ZLength;

        /// <summary>
        ///     The delay line state.
        /// </summary>
        private int ZOffset;

        /// <summary>
        ///     The delay line index to get samples.
        /// </summary>
        private int ZOffsetGet;

        /// <summary>
        ///     The delay line index to set samples.
        /// </summary>
        private readonly int ZOffsetSet;

#if FIR_PROFILE
        private static readonly ProfilerMarker CopyTo1Marker
            = new(ProfilerCategory.Audio, nameof(CopyTo));

        private static readonly ProfilerMarker CopyTo4Marker
            = new(ProfilerCategory.Audio, nameof(CopyTo));

        private static readonly ProfilerMarker ProcessArgsMarker
            = new(ProfilerCategory.Audio, nameof(ProcessArgs));

        private static readonly ProfilerMarker UpdateCenterScalarMarker
            = new(ProfilerCategory.Audio, nameof(UpdateCenterScalar));

        private static readonly ProfilerMarker UpdateCenterVectorMarker
            = new(ProfilerCategory.Audio, nameof(UpdateCenterVector));

        private static readonly ProfilerMarker UpdateZMarker
            = new(ProfilerCategory.Audio, nameof(UpdateZ));
#endif

        private static Filter Create(float[] h, int v, MemoryAllocator allocator)
        {
            var sum0 = h.Where((_, i) => i % 2 == 0).Sum(Math.Abs);
            var sum1 = h.Where((_, i) => i % 2 == 1).Sum(Math.Abs);

            var tap1 = sum0 > sum1 ? 0 : sum1 > sum0 ? 1 : 0;

            return new Filter(h, tap1, v, allocator);
        }

        public static void Free(ref Filter filter, in MemoryAllocator allocator)
        {
            allocator.AlignedFree(new IntPtr(filter.H));
            allocator.AlignedFree(new IntPtr(filter.Z));

            filter = default;
        }

        [BurstCompile]
        private static void CopyTo(in int sample, in int stride, in int offset, in float* target, in float source)
        {
#if FIR_PROFILE
            using var auto = CopyTo1Marker.Auto();
#endif

            target[sample * stride + offset] = source;
        }

        [BurstCompile]
        private static void CopyTo(in int sample, in int stride, in int offset, in float* target, in float4 source)
        {
#if FIR_PROFILE
            using var auto = CopyTo4Marker.Auto();
#endif
            var index0 = sample * stride + offset;
            var index1 = index0 + stride;
            var index2 = index1 + stride;
            var index3 = index2 + stride;

            target[index0] = source[0];
            target[index1] = source[1];
            target[index2] = source[2];
            target[index3] = source[3];
        }

#if FIR_ASSERT // marginal
        [BurstCompile]
        private static void ProcessArgs(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
#if FIR_PROFILE
            using var auto = ProcessArgsMarker.Auto();
#endif

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source),
                    "The pointer to source array is null.");
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target),
                    "The pointer to target array is null.");
            }

            if (length < filter.VLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length,
                    "The length of arrays must be at least of vectorization length.");
            }

            if (length % filter.VLength != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length,
                    "The length of arrays must be a multiple of vectorization length.");
            }

            if (stride < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stride),
                    "The stride must be positive.");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset),
                    "The offset must be positive.");
            }

            if (offset >= stride)
            {
                throw new ArgumentOutOfRangeException(nameof(offset),
                    "The offset must be less than stride.");
            }
        }
#endif

        [BurstCompile]
        private static void UpdateCenterScalar(ref Filter filter, ref float sum)
        {
#if FIR_PROFILE
            using var auto = UpdateCenterScalarMarker.Auto();
#endif

            var h = filter.H;
            var z = filter.Z;
            var c = filter.HCenter;

            var cs = h[c] * z[filter.ZOffsetGet - c];

            sum += filter.HMiddle * cs;
        }

        [BurstCompile]
        private static void UpdateCenterVector(ref Filter filter, ref float4 sum)
        {
#if FIR_PROFILE
            using var auto = UpdateCenterVectorMarker.Auto();
#endif

            var h = filter.H;
            var c = filter.HCenter;
            var z = filter.Z;

            var h0 = h[c];

            var zT = filter.ZOffset + c;

            var z0 = z[zT + 3];
            var z1 = z[zT + 2];
            var z2 = z[zT + 1];
            var z3 = z[zT + 0];

            var v1 = math.float4(h0, h0, h0, h0);
            var v2 = math.float4(z0, z1, z2, z3);
            var v3 = v1 * v2;

            sum += filter.HMiddle * v3;
        }

        [BurstCompile]
        private static int UpdateZ(ref Filter filter, float* source, int sample, int stride, int offset)
        {
            // normally one would need for a call to update the Z offset at the end

            // but by doing stuff in opposite way we can remove the need to have to

            // at the same time we can also hide some details of the implementation

#if FIR_PROFILE
            using var auto = UpdateZMarker.Auto();
#endif

            // update the Z offsets, initial pre-roll is brought back to index zero

            filter.ZOffset -= filter.VLength;

            if (filter.ZOffset < 0)
            {
                filter.ZOffset += filter.ZOffsetSet;
            }

            filter.ZOffsetGet = filter.ZOffset + filter.ZOffsetSet - 1;

            // update the Z line with incoming samples, return the Z offset for use

            var v = filter.VLength;
            var z = filter.Z;

            for (var i = 0; i < v; i++)
            {
                var j = filter.ZOffset + i;
                var k = (j + filter.ZOffsetSet) % filter.ZLength;
                var l = source[(sample + (v - 1 - i)) * stride + offset];

                z[j] = z[k] = l;
            }


            return filter.ZOffsetGet;
        }
    }
}