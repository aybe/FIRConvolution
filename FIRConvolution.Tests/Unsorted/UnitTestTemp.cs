using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FIRConvolution.Tests.Formats.Audio.Extensions;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTestTemp
{
    private bool PrintLine { get; } = false;

    private bool PrintTaps { get; } = false;

    private int Vectors { get; } = 4;

    private float[] TestData { get; set; } = null!;

    private float[] TestTaps { get; set; } = null!;

    private float[] Z { get; set; } = null!;

    private int ZLen { get; set; }

    private int ZPos { get; set; }

    private bool PrintSum { get; } = true;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public TestContext TestContext { get; set; } = null!;

    private int ZMod(int val)
    {
        return (val % Z.Length + Z.Length) % Z.Length;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private int TMod(int val)
    {
        return (val % TestTaps.Length + TestTaps.Length) % TestTaps.Length;
    }

    [TestInitialize]
    public void TestInit()
    {
        var printTaps = false;
        var printData = false;
        var usePulse  = false;

        TestTaps = FilterState.CreateHalfBand(44100, 8820).Coefficients;

        TestData = Enumerable.Range(1, 32).Select(Convert.ToSingle).ToArray();

        if (usePulse)
        {
            TestData    = new float[32];
            TestData[0] = 1;
        }

        Z = new float[(TestTaps.Length + Vectors - 1) * 2];

        ZPos = 0;

        ZLen = Z.Length / 2;

        Assert.AreEqual(26, ZLen);

        if (printTaps)
        {
            TestContext.WriteLine("Taps:");

            for (var i = 0; i < TestTaps.Length; i++)
            {
                TestContext.WriteLine($"{i,2}: {TestTaps[i],16}");
            }
        }

        if (printData)
        {
            TestContext.WriteLine("Data:");
            for (var i = 0; i < TestData.Length; i++)
            {
                TestContext.WriteLine($"{i,2}: {TestData[i],16}");
            }
        }
    }

    [TestMethod]
    public void Test01FullBand()
    {
        var index = 0;

        while (index < TestData.Length)
        {
            Z[ZPos + 3] = Z[ZMod(ZPos + 3 + ZLen)] = TestData[index++];
            Z[ZPos + 2] = Z[ZMod(ZPos + 2 + ZLen)] = TestData[index++];
            Z[ZPos + 1] = Z[ZMod(ZPos + 1 + ZLen)] = TestData[index++];
            Z[ZPos + 0] = Z[ZMod(ZPos + 0 + ZLen)] = TestData[index++];

            if (PrintLine)
            {
                TestContext.WriteLine($"{ZPos,2}: {string.Join(", ", Enumerable.Range(0, Z.Length).Select(s => $"{s,2}"))}");
                TestContext.WriteLine($"{ZPos,2}: {string.Join(", ", Z.Select(s => $"{s,2}"))}");
            }

            var sum = Vector4.Zero;

            for (var tap = 0; tap < TestTaps.Length; tap++)
            {
                var i0 = ZPos + ZLen - 1 - tap - 0;
                var i1 = ZPos + ZLen - 1 - tap - 1;
                var i2 = ZPos + ZLen - 1 - tap - 2;
                var i3 = ZPos + ZLen - 1 - tap - 3;

                var z0 = Z[i0];
                var z1 = Z[i1];
                var z2 = Z[i2];
                var z3 = Z[i3];

                var h0 = TestTaps[tap];

                sum += h0 * new Vector4(z0, z1, z2, z3);

                if (PrintTaps)
                {
                    TestContext.WriteLine($"tap: {tap,2}, zPos: {i0,2}, {i1,2}, {i2,2}, {i3,2}, val: {z0,2}, {z1,2}, {z2,2}, {z3,2}, sum: {sum}");
                }
            }

            if (PrintSum)
            {
                PrintVec(sum);
            }

            ZPos -= Vectors;

            if (ZPos < 0)
            {
                ZPos += ZLen;
            }
        }
    }

    [TestMethod]
    public void Test02HalfBand()
    {
        for (var i = 0; i < TestTaps.Length; i += 1)
        {
            if (i % 2 == 0 || i == TestTaps.Length / 2)
            {
                continue;
            }

            TestTaps[i] = 0;
        }

        var index = 0;

        while (index < TestData.Length)
        {
            Z[ZPos + 3] = Z[ZMod(ZPos + 3 + ZLen)] = TestData[index++];
            Z[ZPos + 2] = Z[ZMod(ZPos + 2 + ZLen)] = TestData[index++];
            Z[ZPos + 1] = Z[ZMod(ZPos + 1 + ZLen)] = TestData[index++];
            Z[ZPos + 0] = Z[ZMod(ZPos + 0 + ZLen)] = TestData[index++];

            if (PrintLine)
            {
                TestContext.WriteLine($"{ZPos,2}: {string.Join(", ", Enumerable.Range(0, Z.Length).Select(s => $"{s,2}"))}");
                TestContext.WriteLine($"{ZPos,2}: {string.Join(", ", Z.Select(s => $"{s,2}"))}");
            }

            var sum = Vector4.Zero;

            for (var tap = 0; tap < TestTaps.Length; tap++)
            {
                var i0 = ZPos + ZLen - 1 - tap - 0;
                var i1 = ZPos + ZLen - 1 - tap - 1;
                var i2 = ZPos + ZLen - 1 - tap - 2;
                var i3 = ZPos + ZLen - 1 - tap - 3;

                var z0 = Z[i0];
                var z1 = Z[i1];
                var z2 = Z[i2];
                var z3 = Z[i3];

                var h0 = TestTaps[tap];

                sum += h0 * new Vector4(z0, z1, z2, z3);

                if (PrintTaps)
                {
                    TestContext.WriteLine($"tap: {tap,2}, zPos: {i0,2}, {i1,2}, {i2,2}, {i3,2}, val: {z0,2}, {z1,2}, {z2,2}, {z3,2}, sum: {sum}");
                }
            }

            if (PrintSum)
            {
                PrintVec(sum);
            }

            ZPos -= Vectors;

            if (ZPos < 0)
            {
                ZPos += ZLen;
            }
        }
    }

    [TestMethod]
    public void Test03HalfBandHalfTaps()
    {
        var index = 0;

        while (index < TestData.Length)
        {
            Z[ZPos + 3] = Z[ZMod(ZPos + 3 + ZLen)] = TestData[index++];
            Z[ZPos + 2] = Z[ZMod(ZPos + 2 + ZLen)] = TestData[index++];
            Z[ZPos + 1] = Z[ZMod(ZPos + 1 + ZLen)] = TestData[index++];
            Z[ZPos + 0] = Z[ZMod(ZPos + 0 + ZLen)] = TestData[index++];

            if (PrintLine)
            {
                TestContext.WriteLine($"{ZPos,2}: {string.Join(", ", Enumerable.Range(0, Z.Length).Select(s => $"{s,2}"))}");
                TestContext.WriteLine($"{ZPos,2}: {string.Join(", ", Z.Select(s => $"{s,2}"))}");
            }

            var sum = Vector4.Zero;

            for (var tap = 0; tap < TestTaps.Length; tap += 2)
            {
                var i0 = ZPos + ZLen - 1 - tap - 0;
                var i1 = ZPos + ZLen - 1 - tap - 1;
                var i2 = ZPos + ZLen - 1 - tap - 2;
                var i3 = ZPos + ZLen - 1 - tap - 3;

                var z0 = Z[i0];
                var z1 = Z[i1];
                var z2 = Z[i2];
                var z3 = Z[i3];

                var h0 = TestTaps[tap];

                //sum += new Vector4(h0 * z0, h0 * z1, h0 * z2, h0 * z3);

                var x = h0 * z0;
                var y = h0 * z1;
                var z = h0 * z2;
                var w = h0 * z3;

                // ReSharper disable once RedundantAssignment
                var testTap = z0 * TestTaps[TMod(tap - 1)] * (index == 0 ? 1 : 0);

                testTap = 0;

                sum += new Vector4(
                    x,
                    y + testTap,
                    z,
                    w
                );

                if (PrintTaps)
                {
                    TestContext.WriteLine(
                        $"tap: {tap,2}, " +
                        $"zPos: " +
                        $"{i0,2}, {i1,2}, {i2,2}, {i3,2}, " +
                        $"val: " +
                        $"{z0,2}, {z1,2}, {z2,2}, {z3,2}, " +
                        $"sum: {sum}");
                }
            }

            var centerIndex = TestTaps.Length / 2;
            var centerValue = TestTaps[centerIndex];
            sum += centerValue *
                   new Vector4(
                       Z[ZPos + centerIndex + 3],
                       Z[ZPos + centerIndex + 2],
                       Z[ZPos + centerIndex + 1],
                       Z[ZPos + centerIndex + 0]
                   );

            if (PrintSum)
            {
                PrintVec(sum);
            }

            ZPos -= Vectors;

            if (ZPos < 0)
            {
                ZPos += ZLen;
            }
        }
    }

    [TestMethod]
    public void Test04HalfBandHalfTapsHalfLoop()
    {
        var index = 0;

        while (index < TestData.Length)
        {
            Z[ZPos + 3] = Z[ZMod(ZPos + 3 + ZLen)] = TestData[index++];
            Z[ZPos + 2] = Z[ZMod(ZPos + 2 + ZLen)] = TestData[index++];
            Z[ZPos + 1] = Z[ZMod(ZPos + 1 + ZLen)] = TestData[index++];
            Z[ZPos + 0] = Z[ZMod(ZPos + 0 + ZLen)] = TestData[index++];

            if (PrintLine)
            {
                TestContext.WriteLine($"{ZPos,2}: {string.Join(", ", Enumerable.Range(0, Z.Length).Select(s => $"{s,2}"))}");
                TestContext.WriteLine($"{ZPos,2}: {string.Join(", ", Z.Select(s => $"{s,2}"))}");
            }

            var sum = Vector4.Zero;

            for (var tap = 0; tap < TestTaps.Length / 2; tap += 2)
            {
                // @formatter:off
                var i0A = ZPos + ZLen - 1 - tap - 0; var i0B = ZPos + ZLen - 1 - (TestTaps.Length - 1 - (tap - 0));
                var i1A = ZPos + ZLen - 1 - tap - 1; var i1B = ZPos + ZLen - 1 - (TestTaps.Length - 1 - (tap - 1));
                var i2A = ZPos + ZLen - 1 - tap - 2; var i2B = ZPos + ZLen - 1 - (TestTaps.Length - 1 - (tap - 2));
                var i3A = ZPos + ZLen - 1 - tap - 3; var i3B = ZPos + ZLen - 1 - (TestTaps.Length - 1 - (tap - 3));

                var z0A = Z[i0A]; var z0B = Z[i0B];
                var z1A = Z[i1A]; var z1B = Z[i1B];
                var z2A = Z[i2A]; var z2B = Z[i2B];
                var z3A = Z[i3A]; var z3B = Z[i3B];
                // @formatter:on

                var h0 = TestTaps[tap];

                sum += new Vector4(h0 * (z0A + z0B), h0 * (z1A + z1B), h0 * (z2A + z2B), h0 * (z3A + z3B));

                if (PrintTaps)
                {
                    TestContext.WriteLine(
                        $"tap: {tap,2}, " +
                        $"zPos: " +
                        $"{i0A,2}, {i1A,2}, {i2A,2}, {i3A,2}, " +
                        $"{i0B,2}, {i1B,2}, {i2B,2}, {i3B,2}, " +
                        $"val: " +
                        $"{z0A,2}, {z1A,2}, {z2A,2}, {z3A,2}, " +
                        $"{z0B,2}, {z1B,2}, {z2B,2}, {z3B,2}, " +
                        $"sum: {sum}");
                }
            }

            var centerIndex = TestTaps.Length / 2;
            var centerValue = TestTaps[centerIndex];
            sum += centerValue *
                   new Vector4(
                       Z[ZPos + centerIndex + 3],
                       Z[ZPos + centerIndex + 2],
                       Z[ZPos + centerIndex + 1],
                       Z[ZPos + centerIndex + 0]
                   );

            if (PrintSum)
            {
                PrintVec(sum);
            }

            ZPos -= Vectors;

            if (ZPos < 0)
            {
                ZPos += ZLen;
            }
        }
    }

    [TestMethod]
    public void Test05NativeFullBand()
    {
        var source = TestData.ToArray();
        var target = new float[source.Length];
        var h      = TestTaps.ToArray();
        var z      = new float[(h.Length + Vectors - 1) * 2];
        var zState = 0;

        FilterFullBand(
            source.AsSpan(), target.AsSpan(), source.Length, 0, 1,
            h.AsSpan(), h.Length, 0,
            z.AsSpan(), z.Length, ref zState
        );

        foreach (var f in target)
        {
            TestContext.WriteLine(f.ToString(CultureInfo.InvariantCulture));
        }
    }

    [TestMethod]
    public void Test06NativeHalfBand()
    {
        var source = TestData.ToArray();
        var target = new float[source.Length];
        var h      = TestTaps.ToArray();
        var z      = new float[(h.Length + Vectors - 1) * 2];
        var zState = 0;

        FilterHalfBand(
            source.AsSpan(), target.AsSpan(), source.Length, 0, 1,
            h.AsSpan(), h.Length, 0,
            z.AsSpan(), z.Length, ref zState
        );

        foreach (var f in target)
        {
            TestContext.WriteLine(f.ToString(CultureInfo.InvariantCulture));
        }
    }

    private void PrintVec(Vector4 sum)
    {
        // ReSharper disable once RedundantIfElseBlock

        if (false)
#pragma warning disable CS0162 // Unreachable code detected
        // ReSharper disable HeuristicUnreachableCode
        {
            TestContext.WriteLine($"sum: {sum}");
        }
        // ReSharper restore HeuristicUnreachableCode
#pragma warning restore CS0162 // Unreachable code detected
        else
        {
            TestContext.WriteLine($"{sum.X}");
            TestContext.WriteLine($"{sum.Y}");
            TestContext.WriteLine($"{sum.Z}");
            TestContext.WriteLine($"{sum.Z}");
        }
    }

    private static void FilterFullBand(
        Span<float> source, Span<float> target, int samples, int channel, int channels,
        Span<float> h, int hCount, int hFirst,
        Span<float> z, int zCount, ref int zState)
    {
        const int vectors = 4;

        var zCenter = zCount / 2;
        var nBlocks = samples / vectors;
        var iSource = channel;
        var iTarget = channel;

        for (var i = 0; i < nBlocks; i++)
        {
            {
                var s0 = source[iSource + channels * 0];
                var s1 = source[iSource + channels * 1];
                var s2 = source[iSource + channels * 2];
                var s3 = source[iSource + channels * 3];

                iSource += channels * 4;

                // @formatter:off
                var i0 = zState + 3; var i4 = ((i0 + zCenter) % zCount + zCount) % zCount;
                var i1 = zState + 2; var i5 = ((i1 + zCenter) % zCount + zCount) % zCount;
                var i2 = zState + 1; var i6 = ((i2 + zCenter) % zCount + zCount) % zCount;
                var i3 = zState + 0; var i7 = ((i3 + zCenter) % zCount + zCount) % zCount;
                // @formatter:on

                z[i0] = z[i4] = s0;
                z[i1] = z[i5] = s1;
                z[i2] = z[i6] = s2;
                z[i3] = z[i7] = s3;
            }

            var sum = Vector4.Zero;

            for (var j = hFirst; j < hCount; j += 1)
            {
                var h0 = h[j];

                var i0 = zState + zCenter - 1 - j - 0;
                var i1 = zState + zCenter - 1 - j - 1;
                var i2 = zState + zCenter - 1 - j - 2;
                var i3 = zState + zCenter - 1 - j - 3;

                var z0 = z[i0];
                var z1 = z[i1];
                var z2 = z[i2];
                var z3 = z[i3];

                sum += new Vector4(h0) * new Vector4(z0, z1, z2, z3);
            }

            for (var j = 0; j < vectors; j++, iTarget += channels)
            {
                target[iTarget] = sum[j];
            }

            zState -= vectors;

            if (zState < 0)
            {
                zState += zCenter;
            }
        }
    }

    private static void FilterHalfBand(
        Span<float> source, Span<float> target, int samples, int channel, int channels,
        Span<float> h, int hCount, int hFirst,
        Span<float> z, int zCount, ref int zState)
    {
        const int vectors = 4;

        var hCenter = hCount / 2;                      // TODO parameter?
        var zCenter = zCount / 2;                      // TODO parameter?
        var tCenter = hFirst is 0 || hCenter % 2 is 0; // TODO parameter?
        var nBlocks = samples / vectors;
        var iSource = channel;
        var iTarget = channel;

        for (var i = 0; i < nBlocks; i++)
        {
            {
                var s0 = source[iSource + channels * 0];
                var s1 = source[iSource + channels * 1];
                var s2 = source[iSource + channels * 2];
                var s3 = source[iSource + channels * 3];

                iSource += channels * 4;

                // @formatter:off
                var i0 = zState + 3; var i4 = ((i0 + zCenter) % zCount + zCount) % zCount;
                var i1 = zState + 2; var i5 = ((i1 + zCenter) % zCount + zCount) % zCount;
                var i2 = zState + 1; var i6 = ((i2 + zCenter) % zCount + zCount) % zCount;
                var i3 = zState + 0; var i7 = ((i3 + zCenter) % zCount + zCount) % zCount;
                // @formatter:on

                z[i0] = z[i4] = s0;
                z[i1] = z[i5] = s1;
                z[i2] = z[i6] = s2;
                z[i3] = z[i7] = s3;
            }

            var sum = Vector4.Zero;

            for (var j = hFirst; j < hCount; j += 2)
            {
                var h0 = h[hCount - 1 - j];

                var i0 = zState + zCenter - 1 - j - 0;
                var i1 = zState + zCenter - 1 - j - 1;
                var i2 = zState + zCenter - 1 - j - 2;
                var i3 = zState + zCenter - 1 - j - 3;

                var z0 = z[i0];
                var z1 = z[i1];
                var z2 = z[i2];
                var z3 = z[i3];

                sum += new Vector4(h0) * new Vector4(z0, z1, z2, z3);
            }

            if (tCenter)
            {
                var h0 = h[hCenter];

                var i0 = zState + hCenter + 3;
                var i1 = zState + hCenter + 2;
                var i2 = zState + hCenter + 1;
                var i3 = zState + hCenter + 0;

                var z0 = z[i0];
                var z1 = z[i1];
                var z2 = z[i2];
                var z3 = z[i3];

                sum += new Vector4(h0) * new Vector4(z0, z1, z2, z3);
            }

            for (var j = 0; j < vectors; j++, iTarget += channels)
            {
                target[iTarget] = sum[j];
            }

            zState -= vectors;

            if (zState < 0)
            {
                zState += zCenter;
            }
        }
    }
}

/*
 * input signal -> filter -> reverb -> mix
 *
 * apply filter to source
 * apply reverb to filter
 * blend result to source
 *
 * source[]
 * filter[]
 * reverb[] can be filter[]
 */

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public struct UnmanagedFilter : IDisposable // TODO
{
    public UnmanagedBuffer<float> Source;
    public UnmanagedBuffer<float> Target;
    public UnmanagedBuffer<float> H;
    public UnmanagedBuffer<float> Z;
    public int ZState;

    public void Dispose()
    {
        Source.Dispose();
        Target.Dispose();
        H.Dispose();
        Z.Dispose();
    }
}

public readonly unsafe struct UnmanagedBuffer<T> // TODO
    : IDisposable
    where T : unmanaged
{
    public readonly T* Items;
    public readonly int Count;
    public readonly int First;
    public readonly int Pitch;

    public UnmanagedBuffer(T[] items, int first = 0, int pitch = 1)
    {
        var sz = Unsafe.SizeOf<T>();
        var cb = sz * items.Length;

        Items = (T*)Marshal.AllocHGlobal(cb);
        Count = items.Length;
        First = first;
        Pitch = pitch;

        items.AsSpan().CopyTo(new Span<T>(Items, Count));
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal((IntPtr)Items);
    }
}