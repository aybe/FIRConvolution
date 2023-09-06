using JetBrains.Annotations;

namespace FIRConvolution.Tests.Unsorted;

[NoReorder]
internal sealed class FilterState1
{
    public readonly float[] H;
    public readonly float[] Z;
    public readonly int[]   T;
    public          int     P;

    private FilterState1(float[] h, int[] t)
    {
        H = h;
        Z = new float[h.Length * 2];
        T = t;
    }

    private static float[] GetCoefficients()
    {
        var f = FilterUtility.LowPass(44100, 11025, 441, FilterWindow.Blackman);

        var h = Array.ConvertAll(f, Convert.ToSingle);

        return h;
    }

    public static FilterState1 CreateFull()
    {
        var h = GetCoefficients();

        var t = Enumerable.Range(0, h.Length).ToArray();

        return new FilterState1(h, t);
    }

    public static FilterState1 CreateHalf()
    {
        var h = GetCoefficients();

        var t = Enumerable.Range(0, h.Length).Where(s => s % 2 == 1 | s == h.Length / 2).ToArray();

        return new FilterState1(h, t);
    }
}