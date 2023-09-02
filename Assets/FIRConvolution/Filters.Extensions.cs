using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;

namespace FIRConvolution
{
    [BurstCompile]
    public static partial class Filters
    {
#if FIR_LOG
        public static Action<object?>? Logger { get; set; }
#endif

        [BurstCompile]
        private static unsafe void CopyTo(in float4 source, in float* target, in int offset)
        {
            *(float4*)(target + offset) = source;
        }

        [BurstCompile]
        private static unsafe void Set(
            in float* source, out float4 target, in int4 indices, [AssumeRange(1, 4)] int components)
        {
            target = float4.zero;

            for (var i = 0; i < components; i++)
            {
                var index = indices[i];
                var value = source[index];
                target[i] = value;
            }
        }

        /// <summary>
        ///     Convolve a scalar value.
        /// </summary>
        /// <param name="fSum">The sum value.</param>
        /// <param name="tIdx">The tap index.</param>
        /// <param name="tNum">The tap count.</param>
        /// <param name="tHop">The tap pitch.</param>
        /// <param name="tMul">The tap multiplier.</param>
        /// <param name="hLen">The components for <paramref name="h" />.</param>
        /// <param name="zLen">The components for <paramref name="z" />.</param>
        /// <param name="zPos">The delay line position.</param>
        /// <param name="h">The coefficients.</param>
        /// <param name="z">The delay line.</param>
        [BurstCompile]
        private static unsafe void Convolve1(
            ref float fSum, ref int tIdx, in int tNum, in int tHop, in int tMul, in int hLen, int zLen, in int zPos, in float* h, in float* z)
        {
            var tVec = new int4(0, 1, 2, 3) * tMul;

            var tEnd = tNum - tHop;

#if FIR_LOG
            Logger?.Invoke(
                $"{nameof(tIdx)}: {tIdx,2}, " +
                $"{nameof(tNum)}: {tNum,2}, " +
                $"{nameof(tHop)}: {tHop,2}, " +
                $"{nameof(hLen)}: {hLen,2}, " +
                $"{nameof(zLen)}: {zLen,2}, " +
                $"{nameof(tMul)}: {tMul,2}, " +
                $"{nameof(zPos)}: {zPos,2}, " +
                $"{nameof(tEnd)}: {tEnd,2}, " +
                $"{nameof(tVec)}: {tVec}, " +
                "");
#endif

            for (; tIdx <= tEnd; tIdx += tHop)
            {
                Loop.ExpectNotVectorized();

                var zGet = zPos - tIdx;

                var hIdx = tIdx + tVec;
                var zIdx = zGet - tVec;

                Set(h, out var hVec, hIdx, hLen);
                Set(z, out var zVec, zIdx, zLen);

                fSum += math.dot(hVec, zVec);

#if FIR_LOG
                Logger?.Invoke(
                    $"{nameof(tIdx)}: {tIdx,2}, " +
                    $"{nameof(hIdx)}: {hIdx}, " +
                    $"{nameof(zIdx)}: {zIdx}, " +
                    $"\n\t{nameof(zGet)}: {zGet,2}, " +
                    $"\n\t{nameof(hVec)}: {hVec}, " +
                    $"\n\t{nameof(zVec)}: {zVec}, " +
                    $"\n\t{nameof(fSum)}: {fSum}");
#endif
            }

#if FIR_LOG
            Logger?.Invoke(string.Empty);
#endif
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