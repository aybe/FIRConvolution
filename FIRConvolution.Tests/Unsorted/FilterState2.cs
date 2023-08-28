using FIRConvolution.Tests.Extensions;
using FIRConvolution.Tests.Formats.Audio.Extensions;
using JetBrains.Annotations;

namespace FIRConvolution.Tests.Unsorted;

[NoReorder]
public sealed class FilterState2
{
    public readonly float2[] H;
    public readonly float2[] Z;
    public readonly int[]    T;
    public          int      P;

    private FilterState2(float2[] h, int[] t)
    {
        H = h;
        Z = new float2[h.Length * 2];
        T = t;
        P = 0;
    }

    private static float[] GetCoefficients()
    {
        var f = Formats.Audio.Extensions.Filter.LowPass(44100, 11025, 441, FilterWindow.Blackman);

        var h = Array.ConvertAll(f, Convert.ToSingle);

        return h;
    }

    public static FilterState2 CreateFull()
    {
        var h = GetCoefficients().ToFloat2().ToArray();

        var t = CreateFullBandTaps(h.Length).ToArray();

        return new FilterState2(h, t);
    }

    public static FilterState2 CreateHalf()
    {
        var h = GetCoefficients().ToFloat2().ToArray();

        var t = CreateHalfBandTaps(h.Length).ToArray();

        return new FilterState2(h, t);
    }

    private static IEnumerable<int> CreateFullBandTaps(int length)
    {
        return Enumerable.Range(0, length);
    }

    private static IEnumerable<int> CreateHalfBandTaps(int length)
    {
        return CreateFullBandTaps(length).Where(s => s % 2 == 1 | s == length / 2);
    }
}