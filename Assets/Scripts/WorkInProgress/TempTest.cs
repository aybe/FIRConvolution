using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class TempTest : MonoBehaviour
{
    [SerializeField]
    private FilterMode FilterMode = FilterMode.Scalar;

    private Filter[] Filters;

    private void OnEnable()
    {
        var components = FilterMode switch
        {
            FilterMode.Scalar => 1,
            FilterMode.Vector => 4,
            _                 => throw new ArgumentOutOfRangeException()
        };

        Filters = GetFilters(components);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        for (var i = 0; i < data.Length; i++)
        {
            var val = i / 2 % 32;
            data[i] = val;
        }

        switch (FilterMode)
        {
            case FilterMode.Scalar:
                PerformScalar(data, channels);
                break;
            case FilterMode.Vector:
                PerformVector(data, channels);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    private void PerformScalar(float[] data, int channels)
    {
        var samples = data.Length / channels;

        for (var channel = 0; channel < channels; channel++)
        {
            var filter = Filters[channel];

            var h       = filter.H;
            var hLength = filter.HLength;
            var z       = filter.Z;

            for (var sample = 0; sample < samples; sample++)
            {
                ref var zOffset = ref filter.ZOffset;

                var inputIndex = sample * channels + channel;

                var input = data[inputIndex];

                z[zOffset] = z[zOffset + hLength] = input;

                var sum = 0.0f;

                for (var tap = 0; tap < hLength; tap++)
                {
                    var h0 = h[tap];
                    var i0 = zOffset + tap;
                    var z0 = z[i0];
                    sum += h0 * z0;
                }

                data[inputIndex] = sum;

                zOffset--;

                if (zOffset < 0)
                {
                    zOffset += hLength;
                }
            }
        }
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    private void PerformVector(float[] data, int channels)
    {
        var samples = data.Length / channels;

        for (var channel = 0; channel < channels; channel++)
        {
            var filter = Filters[channel];

            var cLength = filter.CLength;
            var h       = filter.H;
            var hLength = filter.HLength;
            var z       = filter.Z;
            var zLength = filter.ZLength;

            for (var sample = 0; sample < samples; sample += cLength)
            {
                ref var zOffset = ref filter.ZOffset;

                var inputIndex0 = (sample + 3) * channels + channel;
                var inputIndex1 = (sample + 2) * channels + channel;
                var inputIndex2 = (sample + 1) * channels + channel;
                var inputIndex3 = (sample + 0) * channels + channel;

                var input0 = data[inputIndex0];
                var input1 = data[inputIndex1];
                var input2 = data[inputIndex2];
                var input3 = data[inputIndex3];

                var zIndex0 = zOffset + 0;
                var zIndex1 = zOffset + 1;
                var zIndex2 = zOffset + 2;
                var zIndex3 = zOffset + 3;

                var zIndex4 = (zIndex0 + hLength + (cLength - 1)) % zLength;
                var zIndex5 = (zIndex1 + hLength + (cLength - 1)) % zLength;
                var zIndex6 = (zIndex2 + hLength + (cLength - 1)) % zLength;
                var zIndex7 = (zIndex3 + hLength + (cLength - 1)) % zLength;

                z[zIndex0] = input0;
                z[zIndex1] = input1;
                z[zIndex2] = input2;
                z[zIndex3] = input3;

                z[zIndex4] = input0;
                z[zIndex5] = input1;
                z[zIndex6] = input2;
                z[zIndex7] = input3;

                var sum = float4.zero;

                for (var tap = 0; tap < hLength; tap++)
                {
                    var h0 = h[tap];

                    var i0 = zOffset + tap + 0;
                    var i1 = zOffset + tap + 1;
                    var i2 = zOffset + tap + 2;
                    var i3 = zOffset + tap + 3;

                    i0 = zOffset /*+ (hLength - 1)*/ /* + (cLength - 1)*/ + tap + 3;
                    i1 = zOffset /*+ (hLength - 1)*/ /* + (cLength - 1)*/ + tap + 2;
                    i2 = zOffset /*+ (hLength - 1)*/ /* + (cLength - 1)*/ + tap + 1;
                    i3 = zOffset /*+ (hLength - 1)*/ /* + (cLength - 1)*/ + tap + 0;

                    if (i0 < 0 || i0 >= z.Length || i1 < 0 || i1 >= z.Length || i2 < 0 || i2 >= z.Length || i3 < 0 || i3 >= z.Length)
                    {
                        continue;
                    }

                    var z0 = z[i0];
                    var z1 = z[i1];
                    var z2 = z[i2];
                    var z3 = z[i3];

                    sum += h0 * new float4(z0, z1, z2, z3);
                }

                data[inputIndex0] = sum.x;
                data[inputIndex1] = sum.y;
                data[inputIndex2] = sum.z;
                data[inputIndex3] = sum.w;

                zOffset -= cLength;

                if (zOffset < 0)
                {
                    zOffset += hLength + cLength - 1;
                }
            }
        }
    }

    private static Filter[] GetFilters(int components)
    {
        if (components is < 1 or > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(components));
        }

        var configuration = AudioSettings.GetConfiguration();

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        var channels = configuration.speakerMode switch
        {
            AudioSpeakerMode.Mono        => 1,
            AudioSpeakerMode.Stereo      => 2,
            AudioSpeakerMode.Quad        => 4,
            AudioSpeakerMode.Surround    => 5,
            AudioSpeakerMode.Mode5point1 => 6,
            AudioSpeakerMode.Mode7point1 => 8,
            AudioSpeakerMode.Prologic    => 2,
            _                            => throw new ArgumentOutOfRangeException()
        };

        var filters = new Filter[channels];

        for (var i = 0; i < channels; i++)
        {
            filters[i] = new Filter(23, components);
        }

        return filters;
    }

    private sealed class Filter
    {
        public readonly int CLength;

        public readonly float[] H;

        public readonly int HLength;

        public readonly float[] Z;

        public readonly int ZLength;

        public int ZOffset;

        public Filter(int hLength, int cLength)
        {
            if (hLength % 2 is not 1)
            {
                throw new ArgumentOutOfRangeException(nameof(hLength));
            }

            if (cLength is < 1 or > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(cLength));
            }

            CLength = cLength;

            H = new float[hLength];

            H[0] = 1.0f;

            HLength = H.Length;

            Z = new float[(hLength + (cLength - 1)) * 2];

            ZLength = Z.Length;

            ZOffset = 0;
        }

        public override string ToString()
        {
            return $"{nameof(ZOffset)}: {ZOffset}";
        }
    }
}

[CustomEditor(typeof(TempTest))]
internal sealed class TempTestEditor : Editor
{
}

internal enum FilterMode
{
    Scalar,
    Vector
}