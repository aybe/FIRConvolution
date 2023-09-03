using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Mathematics;

namespace FIRConvolution
{
    [BurstCompile]
    public static partial class Filters
    {
        [BurstCompile]
        private static unsafe void CopyTo(in float4 source, in float* target, in int stride, in int offset)
        {
            for (var i = 0; i < 4; i++)
            {
                target[i * stride + offset] = source[i];
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [AssertionMethod]
        private static unsafe void ValidateArguments(
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
    }
}