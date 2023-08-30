//#define DEBUG_HALF_BAND_START_TAP

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unity.Burst;
using Unity.Mathematics;
#if DEBUG_HALF_BAND_START_TAP
using System.Diagnostics;
#endif

namespace FIRConvolution
{
    [BurstCompile]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public unsafe struct Filter
    {
        public Filter(float[] h, int vLength, int hOffset = 0) // TODO replace by method
        {
            if ((h.Length & 1) == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(h), "Expected an odd number of taps.");
            }

            if (vLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(vLength));
            }

            if (hOffset < 0 || hOffset >= h.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(hOffset));
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
        public int HCenter { get; }

        /// <summary>
        ///     The taps count.
        /// </summary>
        public int HLength { get; }

        /// <summary>
        ///     The first tap offset (for half-band filtering).
        /// </summary>
        public int HOffset { get; }

        /// <summary>
        ///     The center tap multiplier.
        /// </summary>
        public float TCenter { get; } // TODO rename TCenter

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
        public int VLength { get; }

        public static bool TryGetHalfBandStartTap(IReadOnlyCollection<float> taps, out int result)
        {
            var tap0 = taps.Where((_, i) => i % 2 == 0).ToArray();
            var tap1 = taps.Where((_, i) => i % 2 == 1).ToArray();

            var sum0 = tap0.Sum(Math.Abs);
            var sum1 = tap1.Sum(Math.Abs);

#if DEBUG_HALF_BAND_START_TAP
            Debug.WriteLine(nameof(TryGetHalfBandStartTap));
            Debug.WriteLine($"{taps.Count}, {tap0.Length}, {tap1.Length}, {sum0}, {sum1}");
            Debug.WriteLine(nameof(tap0));

            foreach (var f in tap0)
            {
                Debug.WriteLine(f);
            }

            Debug.WriteLine(nameof(tap1));

            foreach (var f in tap1)
            {
                Debug.WriteLine(f);
            }

            Debug.WriteLine(string.Empty);
#endif

            if (sum0 > sum1)
            {
                result = 0;
                return true;
            }

            if (sum1 > sum0)
            {
                result = 1;
                return true;
            }

            result = default;
            return false;
        }

        [BurstCompile]
        public static int UpdateZ(ref Filter filter, float* source, int sample)
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
                var l = source[sample + (v - 1 - i)];

                z[j] = z[k] = l;
            }


            return filter.ZOffsetGet;
        }

        [BurstCompile]
        public static float ProcessCenterScalar(ref Filter filter) // TODO could be a multiplication instead of branching
        {
            var h = filter.H;
            var z = filter.Z;
            var c = filter.HCenter;

            var cs = h[c] * z[filter.ZOffsetGet - c];

            return cs;
        }

        public static float4 ProcessCenterVector(ref Filter filter)
        [BurstCompile]
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

            return v3;
        }
    }
}