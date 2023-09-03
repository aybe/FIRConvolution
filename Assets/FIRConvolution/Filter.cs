using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Mathematics;

namespace FIRConvolution
{
    [BurstCompile]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public unsafe partial struct Filter
    {
        private Filter(float[] h, int vLength, int hOffset)
        {
            if (h.Length % 2 is not 1)
            {
                throw new ArgumentOutOfRangeException(nameof(h),
                    "The number of coefficients must be odd.");
            }

            if (vLength is < 1 or > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(vLength),
                    "The vectorization count must be between 1 and 4.");
            }

            if (hOffset < 0 || hOffset >= h.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(hOffset),
                    "The index to the first coefficient must be a valid coefficient index.");
            }

            var z = new float[(h.Length + (vLength - 1)) * 2];

            var allocator = MemoryAllocator.Current;

            VLength    = vLength;
            H          = allocator.AllocArray(h);
            HLength    = h.Length;
            HCenter    = h.Length / 2;
            Z          = allocator.AllocArray(z);
            ZLength    = z.Length;
            ZOffset    = VLength; // check UpdateZ
            ZOffsetGet = 0;
            ZOffsetSet = HLength + VLength - 1;
            HOffset    = hOffset;
            TCenter    = HCenter % 2 == 1 || HOffset == 1 ? 1.0f : 0.0f;
        }

        /// <summary>
        ///     The taps.
        /// </summary>
        public float* H { get; }

        /// <summary>
        ///     The taps center index.
        /// </summary>
        private int HCenter { get; }

        /// <summary>
        ///     The taps count.
        /// </summary>
        private int HLength { get; }

        /// <summary>
        ///     The first tap offset (for half-band filtering).
        /// </summary>
        private int HOffset { get; }

        /// <summary>
        ///     The center tap multiplier.
        /// </summary>
        private float TCenter { get; } // TODO rename TCenter

        /// <summary>
        ///     The doubled delay line.
        /// </summary>
        public float* Z { get; }

        /// <summary>
        ///     The delay line length.
        /// </summary>
        private int ZLength { get; }

        /// <summary>
        ///     The doubled delay line state.
        /// </summary>
        private int ZOffset { get; set; }

        /// <summary>
        ///     The delay line index to get samples.
        /// </summary>
        private int ZOffsetGet { get; set; }

        /// <summary>
        ///     The delay line index to set samples.
        /// </summary>
        private int ZOffsetSet { get; }

        /// <summary>
        ///     The vectorization count.
        /// </summary>
        private int VLength { get; }

        private static Filter Create(float[] h, int v)
        {
            var sum0 = h.Where((_, i) => i % 2 == 0).Sum(Math.Abs);
            var sum1 = h.Where((_, i) => i % 2 == 1).Sum(Math.Abs);

            var tap1 = sum0 > sum1 ? 0 : sum1 > sum0 ? 1 : 0;

            return new Filter(h, v, tap1);
        }

        [BurstCompile]
        private static void CopyTo(in float4 source, in float* target, in int stride, in int offset)
        {
            for (var i = 0; i < 4; i++)
            {
                target[i * stride + offset] = source[i];
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [AssertionMethod]
        private static void ProcessArgs(
            in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter)
        {
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

            var abs = math.abs(target - source);

            if (abs < length)
            {
                throw new ArgumentException(
                    "The pointers to source and target arrays must not overlap.");
            }

            if (stride < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stride),
                    "Stride must be positive.");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset),
                    "Offset must be positive.");
            }

            if (offset >= stride)
            {
                throw new ArgumentOutOfRangeException(nameof(offset),
                    "Offset must be less than stride.");
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UpdateCenterScalar(ref Filter filter, ref float sum)
        {
            var h = filter.H;
            var z = filter.Z;
            var c = filter.HCenter;

            var cs = h[c] * z[filter.ZOffsetGet - c];

            sum += filter.TCenter * cs;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UpdateCenterVector(ref Filter filter, ref float4 sum)
        {
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

            sum += filter.TCenter * v3;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int UpdateZ(ref Filter filter, float* source, int sample, int stride, int offset)
        {
            // normally one would need for a call to update the Z offset at the end

            // but by doing stuff in opposite way we can remove the need to have to

            // at the same time we can also hide some details of the implementation


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