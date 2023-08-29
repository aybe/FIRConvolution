#define USE_ARRAYS
#define USE_LOOPED
using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
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
    }
}