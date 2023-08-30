#define USE_ARRAYS
#define USE_LOOPED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
#if DEBUG_HALF_BAND_START_TAP
using System.Diagnostics;
#endif

namespace FIRConvolution
{
    [BurstCompile]
    public static partial class Filters
    {
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void CopyTo(in float4 source, in float* target, in int offset)
        {
            *(float4*)(target + offset) = source;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Set(
#if USE_ARRAYS
            in float[] source,
#else
        in Span<float> source,
#endif
            ref float4 target, in int4 indices, int len = 4)
        {
#if USE_LOOPED
            for (var i = 0; i < len; i++)
            {
                target[i] = source[indices[i]];
            }

            for (var i = len; i < 4; i++)
            {
                target[i] = default;
            }
#else
        #error not valid anymore because of the above
        target[0] = source[indices[0]];
        target[1] = source[indices[1]];
        target[2] = source[indices[2]];
        target[3] = source[indices[3]];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Set(
#if USE_ARRAYS
            ref float[] source,
#else
        ref Span<float> source,
#endif
            ref float2 target, in int2 indices)
        {
#if USE_LOOPED
            for (var i = 0; i < 2; i++)
            {
                target[i] = source[indices[i]];
            }
#else
        target[0] = source[indices[0]];
        target[1] = source[indices[1]];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this ref int2 vector, int i) // TODO move
        {
            vector.x = i;
            vector.y = i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this ref int2 vector, int x, int y) // TODO move
        {
            vector.x = x;
            vector.y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Set(this ref int4 vector, int i) // TODO move
        {
            vector.x = i;
            vector.y = i;
            vector.z = i;
            vector.w = i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Set(this ref int4 vector, int x, int y, int z, int w) // TODO move
        {
            vector.x = x;
            vector.y = y;
            vector.z = z;
            vector.w = w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // TODO move
        private static void Convolve1(ref float sum, ref int tap, in int len, in int hop, in int num, in int mul, in int pos, in float[] h, in float[] z)
        {
            var vh = new float4(); // coefficients

            var vz = new float4(); // delays

            var tp = new int4(0, 1, 2, 3) * mul; // taps

            var end = len - hop;

            //Console.WriteLine($"tap: {tap,2}, taps: {tp}, len: {len,2}, hop: {hop,2}, num: {num,2}, mul: {mul,2}, pos: {pos,2}, end: {end,2}");

            for (; tap < end; tap += hop)
            {
                var idx = pos - tap; // Z index

                var iH = tap + tp;
                var iZ = idx - tp;

                var i = num;
                if (tap is 21)
                {
                    Console.WriteLine("z");
                }

                Set(h, ref vh, iH, i);
                Set(z, ref vz, iZ, i);
                sum += math.dot(vh, vz);
                continue;
                Console.WriteLine(
                    $"tap: {tap,2}, " +
                    $"z: {idx,2}, " +
                    $"iH: {iH,-20}, " +
                    $"iZ: {iZ,-20}, " +
                    $"vh: {vh}, " +
                    $"vz: {vz}, " +
                    $"sum: {sum}");
            }

            //Console.WriteLine();
        }

        private static float ProcessCenterScalar(ref Filter filter) // TODO could be a multiplication instead of branching
        {
            var h = filter.H;
            var z = filter.Z;
            var c = filter.HCenter;

            var cs = h[c] * z[filter.ZOffsetGet - c];

            return cs;
        }

        private static float4 ProcessCenterVector(ref Filter filter)
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

        private static unsafe int UpdateZ(ref Filter filter, float* source, int sample)
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
    }
}