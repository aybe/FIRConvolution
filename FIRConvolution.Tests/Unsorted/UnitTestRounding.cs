//#define PRINT_COEFFS

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using FIRConvolution.Tests.Formats.Audio.Extensions;
using FIRConvolution.Tests.Formats.Audio.Microsoft;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public partial class UnitTestRounding
{
    [TestMethod]
    public void TryOldSpeed1()
    {
        const string src = @"C:\Temp\old\test-mono.wav";
        const string dst = @"C:\Temp\old\test-mono-fir-old-speed1.wav";
        FilterBySample(src, dst, s => s.SymmetricalNormal);
    }

    [TestMethod]
    public void TryOldSpeed2()
    {
        const string src = @"C:\Temp\old\test-mono.wav";
        const string dst = @"C:\Temp\old\test-mono-fir-old-speed2.wav";
        FilterBySample(src, dst, s => s.SymmetricalHalf);
    }

    [TestMethod]
    public void TryOldSpeed4()
    {
        const string src = @"C:\Temp\old\test-mono.wav";
        const string dst = @"C:\Temp\old\test-mono-fir-old-speed4.wav";
        FilterBySample(src, dst, s => s.SymmetricalQuarter);
    }

    [TestMethod]
    public void TryOldSpeed8()
    {
        const string src = @"C:\Temp\old\test-mono.wav";
        const string dst = @"C:\Temp\old\test-mono-fir-old-speed8.wav";
        FilterBySample(src, dst, s => s.SymmetricalFinal);
    }

    [TestMethod]
    public void Filter1Normal()
    {
        const string src = @"C:\Temp\old\test-mono.wav";
        const string dst = @"C:\Temp\old\test-mono-fir-new-speed1.wav";
        FilterByChunk(src, dst, s => s.VectorizedNormal);
    }

    [TestMethod]
    public void Filter2Inner()
    {
        const string src = @"C:\Temp\old\test-mono.wav";
        const string dst = @"C:\Temp\old\test-mono-fir-new-speed2.wav";
        FilterByChunk(src, dst, s => s.VectorizedInner);
    }

    [TestMethod]
    public void Filter3Outer()
    {
        const string src = @"C:\Temp\old\test-mono.wav";
        const string dst = @"C:\Temp\old\test-mono-fir-new-speed3.wav";
        FilterByChunk(src, dst, s => s.VectorizedOuter);
    }

    [TestMethod]
    public void Filter4OuterInner()
    {
        const string src = @"C:\Temp\old\test-mono.wav";
        const string dst = @"C:\Temp\old\test-mono-fir-new-speed4.wav";
        FilterByChunk(src, dst, s => s.VectorizedOuterInner);
    }

    [TestMethod]
    public void CreateOffsets()
    {
        PrintOffsets();
    }

    public static void PrintOffsets(int taps = 23, int vectorization = 4)
    {
        var tables = HalfBandFirFilter.GetTables(taps, vectorization);

        foreach (var table in tables)
        {
            Console.WriteLine(string.Join("|", table.Select(s => $"{s,2}")));
        }
    }

    [TestMethod]
    public void TestHalfHalfBandTaps()
    {
        var list = new[] { 11, 13, 15, 17, 19, 21, 23, 459, 461, 463 };

        foreach (var taps in list)
        {
            var center = taps / 2;
            var mirror = center % 2 == 0;

            Console.WriteLine($"{nameof(taps)}: {taps}, {nameof(center)}: {center}, {nameof(mirror)}: {mirror}");

            Console.WriteLine("\titerating 1 in 2 taps NOPE:");

            for (var i = 1; i < taps; i += 2)
            {
                Console.WriteLine($"\t\t{(i == center ? "\tcenter " : "")}{i}");
            }

            Console.WriteLine("\titerating 1 in 4 taps GOOD:");

            for (var i = 1; i < center; i += 2)
            {
                var j = i;
                var k = center + (center - i);
                Console.WriteLine($"\t\t{j} {k}");

                Assert.AreEqual(taps, j + k + 1);
            }

            Console.WriteLine($"\t\tTODO convolve center: {center}");
            Console.WriteLine();
        }
    }

    private static void FilterBySample(string src, string dst, Expression<Func<HalfBandFirFilter, Func<float, float>>> expression, int bw = 441)
    {
        var halfBand = FilterState.CreateHalfBand(44100, bw);

        var h = halfBand.Coefficients;
        Console.WriteLine("H: " + h.Length);
#if PRINT_COEFFS
        for (var index = 0; index < h.Length; index++)
        {
            var f = h[index];
            Console.WriteLine($"\t{index,3}: {f:F18}");
        }
#endif
        var fir = new HalfBandFirFilter(h);


        var func = expression.Compile()(fir);

        using var sourceStream = File.OpenRead(src);
        using var targetStream = File.Create(dst);
        using var sourceWav    = new Wav(sourceStream);
        using var targetWav    = new Wav(targetStream, sourceWav.Channels, sourceWav.BitsPerSample, sourceWav.SampleRate);

        const int bufferLength = 1024;

        var sourceBuffer = sourceWav.CreateBuffer<float>(bufferLength);
        var targetBuffer = sourceWav.CreateBuffer<float>(bufferLength);

        int read;

        do
        {
            read = sourceWav.Read(sourceBuffer);

            for (var i = 0; i < read; i++)
            {
                targetBuffer[i] = func(sourceBuffer[i]);
            }

            targetWav.Write(targetBuffer, 0, read);
        } while (read == bufferLength);
    }

    private static void FilterByChunk(string src, string dst, Expression<Func<HalfBandFirFilter, Action<float[], float[], int>>> expression, int bw = 441)
    {
        var halfBand = FilterState.CreateHalfBand(44100, bw);

        var h = halfBand.Coefficients;
        Console.WriteLine("H: " + h.Length);
#if PRINT_COEFFS
        for (var index = 0; index < h.Length; index++)
        {
            var f = h[index];
            Console.WriteLine($"\t{index,3}: {f:F18}");
        }
#endif
        var fir = new HalfBandFirFilter(h);


        var func = expression.Compile()(fir);

        using var sourceStream = File.OpenRead(src);
        using var targetStream = File.Create(dst);
        using var sourceWav    = new Wav(sourceStream);
        using var targetWav    = new Wav(targetStream, sourceWav.Channels, sourceWav.BitsPerSample, sourceWav.SampleRate);

        const int bufferLength = 1024;

        var sourceBuffer = sourceWav.CreateBuffer<float>(bufferLength);
        var targetBuffer = sourceWav.CreateBuffer<float>(bufferLength);

        int read;

        do
        {
            read = sourceWav.Read(sourceBuffer);

            func(sourceBuffer, targetBuffer, bufferLength);

            targetWav.Write(targetBuffer, 0, read);
        } while (read == bufferLength);
    }
}

public partial class UnitTestRounding
{
    [TestMethod]
    public void TestShort2Float2Short()
    {
        foreach (var i in Enumerable.Range(short.MinValue, short.MaxValue - short.MinValue + 1))
        {
            Assert.AreEqual(i, i / 32768.0f * 32768.0f);
        }
    }

    [TestMethod]
    public void TryVectorizeFilter()
    {
        var fs = FilterState.CreateHalfBand();

        Console.WriteLine($"h: {fs.Coefficients.Length}");
        Console.WriteLine($"z: {fs.DelayLine.Length}");
        Console.WriteLine($"taps: {fs.Taps.Length}");
        foreach (var tap in fs.Taps)
        {
            Console.WriteLine($"{tap}: {fs.Coefficients[tap]:E}");
        }

        Console.WriteLine();

        var source = new float[1234];
        var target = new float[1234];

        Convolve(source, target, fs.Coefficients, fs.DelayLine, ref fs.Position);
    }

    private static void Convolve(
        // ReSharper disable once UnusedParameter.Local
        Span<float> source, Span<float> target, Span<float> h, Span<float> z, ref int state)
    {
        Assert.AreEqual(source.Length, target.Length);
        Assert.AreEqual(h.Length * 2, z.Length);

        var len1 = source.Length / 4;
        var len2 = source.Length % 4;

        Console.WriteLine($"source length: {source.Length}");
        Console.WriteLine($"chunks of 4: {len1}");
        Console.WriteLine($"leftovers: {len2}");

        var source4 = MemoryMarshal.Cast<float, float4>(source);
        var target4 = MemoryMarshal.Cast<float, float4>(target);

        for (var i = 0; i < len1; i++)
        {
            target4[i] = source4[i];
        }

        for (var i = len1 * 4; i < source.Length; i++)
        {
            Console.WriteLine(i);
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static void Mul222(
        Span<float2> source1, Span<float2> source2, Span<float2> target, float dry, float wet)
    {
        var dry2 = new float2(dry * 0.5f);
        var wet2 = new float2(wet * 0.5f);

        for (var i = 0; i < source1.Length; i++)
        {
            var sample1 = source1[i];
            var sample2 = source2[i];

            target[i] = sample1 * dry2 + sample2 * wet2;
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static void Mul42(
        Span<float4> source, Span<float2> target, float4 mix)
    {
        mix *= 0.5f;

        for (var i = 0; i < source.Length; i++)
        {
            var src = source[i] * mix;

            target[i] = new float2(src.x + src.y, src.z + src.w);
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static void Mul44(
        Span<float4> source, Span<float4> target, float4 mix)
    {
        mix *= 0.5f;

        for (var i = 0; i < source.Length; i++)
        {
            target[i] = source[i] * mix;
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static void Mix42(
        Span<float4> source, Span<float2> target)
    {
        for (var i = 0; i < source.Length; i++)
        {
            var src = source[i];

            target[i] = new float2(src.x + src.y, src.z + src.w);
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public void Filter1(Span<float> source, Span<float> target, int length)
    {
        Assert.AreEqual(length, source.Length);
        Assert.AreEqual(length, target.Length);

        for (var i = 0; i < length; i++)
        {
            var input = source[i];

            target[i] = input;
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public void Filter4(Span<float> source, Span<float> target, int length)
    {
        Assert.AreEqual(length, source.Length);
        Assert.AreEqual(length, target.Length);
        Assert.AreEqual(0, length % 4); // BUG?

        var source4 = MemoryMarshal.Cast<float, float4>(source);
        var target4 = MemoryMarshal.Cast<float, float4>(target);
        var length4 = length / 4;

        for (var i = 0; i < length4; i++)
        {
            var s = source4[i];

            // TODO

            target4[i] = s;
        }
    }
}