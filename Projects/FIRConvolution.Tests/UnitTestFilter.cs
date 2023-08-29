using System.ComponentModel;
using FIRConvolution.Tests.Formats.Audio.Extensions;
using JetBrains.Annotations;

namespace FIRConvolution.Tests;

public abstract class UnitTestFilter
{
    public required TestContext TestContext { get; [UsedImplicitly] set; }

    private const float TestDelta = 1E-05f;

    private static int TestIterations
    {
        get
        {
#if DEBUG
            return 1;
#else
            return 1_000_000;
#endif
        }
    }

    private static UnitTestFilterSignal TestSignal => UnitTestFilterSignal.Ascending;

    public static float[] GetInput(UnitTestFilterSignal signal, int repeat = 1)
    {
        var source = signal switch
        {
            UnitTestFilterSignal.Pulse     => new float[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            UnitTestFilterSignal.Triangle  => new float[] { 1, 2, 3, 4, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            UnitTestFilterSignal.Ascending => new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 },
            _                              => throw new InvalidEnumArgumentException(nameof(signal), (int)signal, typeof(UnitTestFilterSignal))
        };

        var target = Enumerable.Repeat(source, repeat).SelectMany(s => s).ToArray();

        return target;
    }

    private static unsafe List<float> MakeFilter(float[] input, float[] taps, UnitTestFilterFactory<Filter> factory, FilterMethod filterMethod)
    {
        const int blockSize = 16;

        var output = new List<float>();

        var filter = factory(taps);

        Span<float> target = stackalloc float[blockSize];

        for (var block = 0; block < input.Length / blockSize; block++)
        {
            var source = input.AsSpan(block * blockSize, blockSize);

            target.Clear();

            fixed (float* pSource = source)
            fixed (float* pTarget = target)
            {
                filterMethod(pSource, pTarget, blockSize, ref filter);
            }

            foreach (var sample in target)
            {
                output.Add(sample);
            }
        }

        return output;
    }

    protected unsafe void TestFilter(UnitTestFilterFactory<Filter> factory, FilterMethod filterMethod, int bandwidth)
    {
#if !DEBUG
        TestContext.WriteLine("Not printing stats for release configuration!"); // OOM otherwise
#endif

        var taps = FilterState.CreateHalfBand(44100.0d, bandwidth).Coefficients;

        var input = GetInput(TestSignal, TestIterations);

        var actual = MakeFilter(input, taps, factory, filterMethod);

        var expected = MakeFilter(input, taps, FilterFactory.CreateScalarFullBand, Filters.ScalarFullBand);

#if DEBUG
        TestContext.WriteLine($"{nameof(taps)}: {taps.Length}, {nameof(input)}: {input.Length}");
#endif

        Assert.AreEqual(expected.Count, actual.Count, "Count mismatch.");

#if DEBUG
        TestContext.WriteLine($"{"index",8} {"expected",16} {"actual",16} {"difference",16} {"deviation",16} {"match",8}");
#endif

        var errors = 0;

        for (var i = 0; i < expected.Count; i++)
        {
            var expectedValue = expected[i];
            var actualValue   = actual[i];
            var difference    = Math.Abs(expectedValue - actualValue);
            var deviation     = FormatPercent(TestDelta, 1 - actualValue / expectedValue);
            var failed        = difference > TestDelta;

#if DEBUG
            TestContext.WriteLine($"{i,8} {expectedValue,16} {actualValue,16} {difference,16:E} {deviation,16} {!failed,8}");
#endif

            if (failed)
            {
                errors++;
            }
        }

        if (errors <= 0)
        {
            return;
        }

        var percentage = (double)errors / actual.Count;

        Assert.Fail($"ERRORS: {errors}/{actual.Count}, PASS: {1 - percentage:P}, FAIL: {percentage:P}");
    }

    private static string FormatPercent(in float delta, in float value)
    {
        var s = delta.ToString("F16");
        var i = s.IndexOf('.');
        var j = s.LastIndexOf('0');
        var k = j - i + 1;
        var l = value.ToString($"P{k}");

        return l;
    }
}