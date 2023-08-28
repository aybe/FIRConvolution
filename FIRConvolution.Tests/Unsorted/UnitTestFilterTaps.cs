using System.Numerics;
using FIRConvolution.Extensions;
using ServiceStack;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public sealed class UnitTestFilterTaps
{
    [TestMethod]
    public void TestTaps4()
    {
        TestTaps(4);
    }

    [TestMethod]
    public void TestTaps8()
    {
        TestTaps(8);
    }

    private static void TestTaps(int vectors)
    {
        Assert.IsTrue(Vector.IsHardwareAccelerated, "Vector.IsHardwareAccelerated");

        const int start = 21;
        const int end   = 461;
        const int hop   = 2;

        var list = new List<FilterStats>();

        for (var taps = start; taps <= end; taps += hop)
        {
            var stats = TestTaps(vectors, taps);
            list.Add(stats);
        }

        Console.WriteLine(list.ToCsv());
    }

    private static FilterStats TestTaps(int vectors, int tapCount)
    {
        var stats = new FilterStats(vectors, tapCount);

        const int hop = 2;

        var len = stats.CenterTap - stats.Delays + stats.Vectors;
        var vec = stats.Vectors - 1;
        var pct = (float)(len - hop + vec) / stats.CenterTap;

        Console.WriteLine($"\t{nameof(tapCount)}: {tapCount}, {nameof(hop)}: {hop}, {nameof(len)}: {len}, {nameof(vec)}: {vec}, {nameof(pct)}: VEC: {pct:P}, NRM: {1 - pct:P}");

        for (var tap = 0; tap < len; tap += hop)
        {
            var tL1 = tap;
            var tL2 = tL1 + vec;
            var tR1 = stats.Taps - 1 - tL1;
            var tR2 = tR1 + vec;

            Console.WriteLine($"\t\t{nameof(tap)}: {tap,3}, L: [{tL1,3},{tL2,3}], R: [{tR1,3},{tR2,3}]");

            Assert.That.IsLessThan(stats.CenterTap, tL1);
            Assert.That.IsLessThan(stats.CenterTap, tL2);
            Assert.That.IsGreaterThan(stats.CenterTap, tR1);
            Assert.That.IsGreaterThan(stats.CenterTap, tR2);
            Assert.That.IsLessThan(stats.Taps, tR1);
        }

        return stats;
    }
}

public sealed class FilterStats
{
    private const int HalfBandStep = 2;

    public FilterStats(int vectors, int taps)
    {
        if (vectors <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(vectors));
        }

        if (taps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taps));
        }

        if (taps % 2 == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taps));
        }

        Vectors   = vectors;
        Taps      = taps;
        CenterTap = Taps / 2;
        Delays    = Vectors * HalfBandStep - 1;

        const int hop = 2;

        var len = CenterTap - Delays + Vectors;

        var vec = Vectors - 1;

        PercentageVector = (double)(len - hop + vec) / CenterTap;
        PercentageNormal = 1.0d - PercentageVector;
    }

    public int Vectors { get; }

    public int Taps { get; }

    public int CenterTap { get; }

    public int Delays { get; }

    public double PercentageNormal { get; set; }

    public double PercentageVector { get; }
}