using System.Numerics;
using FIRConvolution.Tests.Formats.Audio.Extensions;
using FIRConvolution.WorkInProgress;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTestNative
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void TestMethod1()
    {
        var h = FilterState.CreateHalfBand(bw: 441).Coefficients;
        var source = UnitTestFilter.GetInput(UnitTestFilterSignal.Pulse,128);
        source    = new float[BitOperations.RoundUpToPowerOf2((uint)h.Length)];
        source[0] = 1;
        var target = new float[source.Length];

        TestContext.WriteLine($"{h.Length}");
        var filter = NativeFilterFactory.Create(h, 4);

        NativeFilterProcess.FullBandOuterInner(source, target, source.Length, 1, 0, ref filter);

        NativeFilterFactory.Dispose(ref filter);

        var i = 0;
        foreach (var (a, b) in h.Zip(target))
        {
            var abs = Math.Abs(a - b);
            TestContext.WriteLine($"{i++,4}, {a,16}, {b,16}, {abs,16:E}, {1 - abs / a,16:P}, {abs < 1e-5}");
        }
    }

    [TestMethod]
    public void TestMethod2()
    {
        //     0123
        // --------
        // 1 0 0123
        // 2 0 0246
        // 2 1 1357

        TestContext.WriteLine(string.Empty);

        for (var stride = 1; stride <= 2; stride++)
        {
            for (var offset = 0; offset <= stride - 1; offset++)
            {
                for (var sample = 0; sample < 4; sample += 4)
                {
                    var s0 = sample + 0;
                    var s1 = sample + 1;
                    var s2 = sample + 2;
                    var s3 = sample + 3;

                    var t0 = (sample + 0) * stride + offset;
                    var t1 = (sample + 1) * stride + offset;
                    var t2 = (sample + 2) * stride + offset;
                    var t3 = (sample + 3) * stride + offset;
                    TestContext.WriteLine($"{nameof(stride)}: {stride}, {nameof(offset)}: {offset}, {nameof(sample)}: {sample}");
                    TestContext.WriteLine($"\t{s0,2}, {s1,2}, {s2,2}, {s3,2} -> {t0,2}, {t1,2}, {t2,2}, {t3,2}");
                }

                TestContext.WriteLine(string.Empty);
            }
        }
    }
}