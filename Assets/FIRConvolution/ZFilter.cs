// #define FIR_LOG_INPUT
// #define FIR_LOG_TAP
// #define FIR_LOG_Z

// BUG logging when processing large input will make Live Unit Testing abandon at some point

using System;
using System.Diagnostics;
using System.Linq;
using Unity.Mathematics;

// TODO

namespace FIRConvolution.Tests
{
    public sealed partial class ZFilter
    {
        private readonly float[] H;

        private readonly int HLength;

        private readonly float[] Z;

        private readonly int ZLength;

        private readonly int ZMirror;

        private readonly int ZVector;

        private int ZOffset;

        public ZFilter(float[] h, int zVector)
        {
            var hLength = h.Length;

            if (hLength % 2 is not 1)
            {
                throw new ArgumentOutOfRangeException(nameof(h));
            }

            if (zVector is < 1 or > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(zVector));
            }

            var zLength = (hLength + (zVector - 1)) * 2;

            H = h.ToArray();

            HLength = hLength;

            Z = new float[zLength];

            ZLength = zLength;

            ZMirror = hLength + zVector - 1;

            ZOffset = 0;

            ZVector = zVector;
        }

        private static void UpdateZOffset(ref ZFilter filter)
        {
            ref var zOffset = ref filter.ZOffset;

            zOffset -= filter.ZVector;

            if (zOffset < 0)
            {
                zOffset += filter.ZMirror;
            }
        }

        public override string ToString()
        {
            return $"{nameof(ZOffset)}: {ZOffset}";
        }
    }

    public sealed partial class ZFilter
    {
        public static Action<object?>? Logger { get; set; }

        [Conditional("FIR_LOG_INPUT")]
        private static void LogInput(object? o)
        {
            Logger?.Invoke(o);
        }

        [Conditional("FIR_LOG_TAP")]
        private static void LogTap(object? o)
        {
            Logger?.Invoke(o);
        }

        [Conditional("FIR_LOG_Z")]
        private static void LogZ(ref ZFilter filter)
        {
            var zOffset = filter.ZOffset;

            Logger?.Invoke($"{nameof(zOffset)}: {zOffset}");

            var z = filter.Z;

            Logger?.Invoke(string.Join(", ", z.Select((_, t) => $"{t,2}")));
            Logger?.Invoke(string.Join(", ", z.Select((s, _) => $"{s,2}")));
        }
    }

    public sealed partial class ZFilter
    {
        public static unsafe void ProcessFilterScalar(
            in float* source, in float* target, in int length, in int stride, in int offset, ref ZFilter filter)
        {
            var h       = filter.H;
            var hLength = filter.HLength;
            var z       = filter.Z;
            var zMirror = filter.ZMirror;
            var zVector = filter.ZVector;

            ref var zOffset = ref filter.ZOffset;

            for (var sample = 0; sample < length; sample += zVector)
            {
                var sampleIndex0 = (sample + 0) * stride + offset;

                var sampleValue0 = source[sampleIndex0];

                LogInput($"{nameof(sample)}: {sample,2}, {nameof(sampleIndex0)}, {sampleIndex0,2}, {nameof(sampleValue0)}, {sampleValue0,2}");

                z[zOffset] = z[zOffset + zMirror] = sampleValue0;

                LogZ(ref filter);

                var sum = 0.0f;

                for (var tap = 0; tap < hLength; tap++)
                {
                    var h0 = h[tap];
                    var i0 = zOffset + tap;
                    var z0 = z[i0];

                    sum += h0 * z0;

                    LogTap($"{nameof(zOffset)}: {zOffset,2}, {nameof(tap)}: {tap,2}, indices: {i0,2}, values: {z0,2}");
                }

                target[sampleIndex0] = sum;

                UpdateZOffset(ref filter);
            }
        }
    }

    public sealed partial class ZFilter
    {
        public static unsafe void ProcessFilterVector(
            in float* source, in float* target, in int length, in int stride, in int offset, ref ZFilter filter)
        {
            var h       = filter.H;
            var hLength = filter.HLength;
            var z       = filter.Z;
            var zLength = filter.ZLength;
            var zMirror = filter.ZMirror;
            var zVector = filter.ZVector;

            ref var zOffset = ref filter.ZOffset;

            for (var sample = 0; sample < length; sample += zVector)
            {
            // @formatter:off

            var sampleIndex0 = (sample + 0) * stride + offset; var sampleValue0 = source[sampleIndex0];
            var sampleIndex1 = (sample + 1) * stride + offset; var sampleValue1 = source[sampleIndex1];
            var sampleIndex2 = (sample + 2) * stride + offset; var sampleValue2 = source[sampleIndex2];
            var sampleIndex3 = (sample + 3) * stride + offset; var sampleValue3 = source[sampleIndex3];

            var zIndex0 = zOffset + 0; var zIndex4 = (zIndex0 + zMirror) % zLength;
            var zIndex1 = zOffset + 1; var zIndex5 = (zIndex1 + zMirror) % zLength;
            var zIndex2 = zOffset + 2; var zIndex6 = (zIndex2 + zMirror) % zLength;
            var zIndex3 = zOffset + 3; var zIndex7 = (zIndex3 + zMirror) % zLength;

            LogInput($"{nameof(sample)}: {sample,2}" +
                $"\n{nameof(sampleIndex0)}, {sampleIndex0,2}, {nameof(sampleIndex1)}, {sampleIndex1,2}, {nameof(sampleIndex2)}, {sampleIndex2,2}, {nameof(sampleIndex3)}, {sampleIndex3,2}" +
                $"\n{nameof(sampleValue0)}, {sampleValue0,2}, {nameof(sampleValue1)}, {sampleValue1,2}, {nameof(sampleValue2)}, {sampleValue2,2}, {nameof(sampleValue3)}, {sampleValue3,2}");
            
            z[zIndex0] = sampleValue3; z[zIndex4] = sampleValue3;
            z[zIndex1] = sampleValue2; z[zIndex5] = sampleValue2;
            z[zIndex2] = sampleValue1; z[zIndex6] = sampleValue1;
            z[zIndex3] = sampleValue0; z[zIndex7] = sampleValue0;
            
                // @formatter:on

                LogZ(ref filter);

                var sum = default(float4);

                for (var tap = 0; tap < hLength; tap++)
                {
                    var h0 = h[tap];

                    var zT = zOffset + tap;

                    var i0 = zT + 3;
                    var i1 = zT + 2;
                    var i2 = zT + 1;
                    var i3 = zT + 0;

                    var z0 = z[i0];
                    var z1 = z[i1];
                    var z2 = z[i2];
                    var z3 = z[i3];

                    var hv = new float4(h0, h0, h0, h0);
                    var zv = new float4(z0, z1, z2, z3);

                    sum += hv * zv;

                    LogTap($"{nameof(zOffset)}: {zOffset,2}, {nameof(tap)}: {tap,2}, indices: {i0,2}, {i1,2}, {i2,2}, {i3,2}, values: {z0,2}, {z1,2}, {z2,2}, {z3,2}");
                }

                target[sampleIndex0] = sum[0];
                target[sampleIndex1] = sum[1];
                target[sampleIndex2] = sum[2];
                target[sampleIndex3] = sum[3];

                UpdateZOffset(ref filter);
            }
        }
    }
}