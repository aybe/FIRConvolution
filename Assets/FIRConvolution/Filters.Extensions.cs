#define USE_ARRAYS
#define USE_LOOPED
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static partial class Filters
    {
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
        public static void Set(this ref int4 vector, int i) // TODO move
        {
            vector.x = i;
            vector.y = i;
            vector.z = i;
            vector.w = i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this ref int4 vector, int x, int y, int z, int w) // TODO move
        {
            vector.x = x;
            vector.y = y;
            vector.z = z;
            vector.w = w;
        }
    }
}