#define PRINT_Z_LINE_IDX
#define PRINT_Z_LINE_VAL
using System.Diagnostics;
using System.Numerics;
using FIRConvolution.Extensions;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTestFiltering
{
    private const double TestDelta = 1E-5;

    private static readonly float[] TestData = Enumerable.Range(1, 32).Select(Convert.ToSingle).ToArray();

    private static readonly float[] TestTaps =
    {
        0.0000000000000000f, 0.0000000000000000f, 0.0011532464995980f, -0.0000000000000000f,
        -0.0072714607231319f, 0.0000000000000000f, 0.0263820774853230f, -0.0000000000000000f,
        -0.0780988410115242f, 0.0000000000000000f, 0.3078285157680511f,
        0.5000129342079163f,
        0.3078285157680511f, 0.0000000000000000f, -0.0780988410115242f, -0.0000000000000000f,
        0.0263820774853230f, 0.0000000000000000f, -0.0072714607231319f, -0.0000000000000000f,
        0.0011532464995980f, 0.0000000000000000f, 0.0000000000000000f
    };

    private static readonly float[] TestPass =
    {
        0.0000000000000000f, 0.0000000000000000f, 0.0011532464995980f, 0.0023064929991961f,
        -0.0038117212243378f, -0.0099299354478717f, 0.0103339282795787f, 0.0305977910757065f,
        -0.0272371806204319f, -0.0850721672177315f, 0.1649213731288910f, 0.9149278998374939f,
        1.9727628231048584f, 3.0305981636047363f, 4.0103340148925781f, 4.9900698661804199f,
        5.9961891174316406f, 7.0023069381713867f, 8.0011539459228516f, 9.0000000000000000f,
        9.9999990463256836f, 11.0000000000000000f, 12.0000009536743164f, 13.0000000000000000f,
        14.0000000000000000f, 15.0000009536743164f, 16.0000000000000000f, 17.0000000000000000f,
        18.0000019073486328f, 19.0000000000000000f, 19.9999980926513672f, 20.9999980926513672f
    };

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void TestFullBand()
    {
        Test(DoFullBand);
    }

    [TestMethod]
    public void TestHalfBand()
    {
        Test(DoHalfBand);
    }

    [TestMethod]
    public void TestHalfBandHalfTaps()
    {
        Test(DoHalfBandHalfTaps);
    }

    [TestMethod]
    public void TestHalfBandHalfTapsInner()
    {
        Test(DoHalfBandHalfTapsInner);
    }

    [TestMethod]
    public void TestHalfBandHalfTapsOuter()
    {
        Test(DoHalfBandHalfTapsOuter);
    }

    private void Test(Func<float[], float[]> handler)
    {
        TestContext.WriteLine($"taps: {TestTaps.Length}, input: {TestData.Length}, delta: {TestDelta}");

        var source = TestData;
        var result = handler(source);

        Assert.AreEqual(source.Length, result.Length, "Length mismatch.");

        var errors = 0;
        var offset = 0;

        TestContext.WriteLine("index, expected, actual, difference, deviation, status");

        foreach (var (expected, actual) in TestPass.Zip(result))
        {
            var abs = Math.Abs(expected - actual);

            var error = abs > TestDelta;

            if (error)
            {
                errors++;
            }

            var dev = actual / expected;

            var status = error ? "FAIL" : "PASS";

            TestContext.WriteLine(
                $"{offset++,2}, " +
                $"{expected,16:F12}, " +
                $"{actual,16:F12}, " +
                $"{abs,16:F12}, " +
                $"{1 - dev,20:P8}, " +
                $"{status}");
        }

        if (errors != 0)
        {
            var percentage = (double)errors / source.Length;

            Assert.Fail($"PASS: {1 - percentage:P}, FAIL: {percentage:P}");
        }
    }

    /// <summary>
    ///     naive, reference.
    /// </summary>
    private static float[] DoFullBand(float[] source)
    {
        var result  = new float[source.Length];
        var z       = new float[TestTaps.Length * 2];
        var zOffset = 0;
        var hLength = TestTaps.Length;


        for (var i = 0; i < source.Length; i++)
        {
            z[zOffset] = z[zOffset + hLength] = source[i];

            var f = 0.0f;

            for (var j = 0; j < hLength; j++)
            {
                var h0 = TestTaps[j];
                var z0 = z[zOffset + hLength - 1 - j];
                f += h0 * z0;
            }

            result[i] = f;

            zOffset--;

            if (zOffset < 0)
            {
                zOffset += hLength;
            }
        }

        return result;
    }

    /// <summary>
    ///     half band, 1 tap in 2, full loop
    /// </summary>
    private static float[] DoHalfBand(float[] source)
    {
        var result  = new float[source.Length];
        var z       = new float[TestTaps.Length * 2];
        var zOffset = 0;
        var hLength = TestTaps.Length;
        var hCenter = hLength / 2;

        for (var i = 0; i < source.Length; i++)
        {
            z[zOffset] = z[zOffset + hLength] = source[i];

            var f = 0.0f;

            for (var j = 0; j < hLength; j += 2)
            {
                var h0 = TestTaps[j];
                var z0 = z[zOffset + hLength - 1 - j];
                f += h0 * z0;
            }

            f += TestTaps[hCenter] * z[zOffset + hCenter];

            result[i] = f;

            zOffset--;

            if (zOffset < 0)
            {
                zOffset += hLength;
            }
        }

        return result;
    }

    /// <summary>
    ///     half band, 1 tap in 2, half loop
    /// </summary>
    private static float[] DoHalfBandHalfTaps(float[] source)
    {
        var result  = new float[source.Length];
        var z       = new float[TestTaps.Length * 2];
        var zOffset = 0;
        var hLength = TestTaps.Length;
        var hCenter = hLength / 2;

        for (var i = 0; i < source.Length; i++)
        {
            z[zOffset] = z[zOffset + hLength] = source[i];

            var f = 0.0f;

            for (var j = 0; j < hCenter; j += 2)
            {
                var h0 = TestTaps[j];
                var z0 = z[zOffset + hLength - 1 - j];
                var z1 = z[zOffset + hLength - 1 - (hLength - 1 - j)];
                f += h0 * (z0 + z1);
            }

            f += TestTaps[hCenter] * z[zOffset + hCenter];

            result[i] = f;

            zOffset--;

            if (zOffset < 0)
            {
                zOffset += hLength;
            }
        }

        return result;
    }

    /// <summary>
    ///     half band, 1 tap in 2, half loop, vectorized outer loop
    /// </summary>
    private float[] DoHalfBandHalfTapsOuter(float[] source)
    {
        var result  = new float[source.Length];
        var z       = new float[(TestTaps.Length + 3) * 2];
        var zOffset = 0;
        var hLength = TestTaps.Length;
        var hCenter = hLength / 2;

        for (var i = 0; i < source.Length; i += 4)
        {
            z[zOffset + 0] = z[(zOffset + 0 + hLength + 3) % z.Length] = source[i + 3];
            z[zOffset + 1] = z[(zOffset + 1 + hLength + 3) % z.Length] = source[i + 2];
            z[zOffset + 2] = z[(zOffset + 2 + hLength + 3) % z.Length] = source[i + 1];
            z[zOffset + 3] = z[(zOffset + 3 + hLength + 3) % z.Length] = source[i + 0];

            PrintZLineIdx(z);

            var v = Vector4.Zero;

            for (var j = 0; j < hCenter; j += 2)
            {
                var h0 = TestTaps[j];

                var i0 = zOffset + z.Length / 2 - j - 1;
                var i1 = i0 - 1;
                var i2 = i1 - 1;
                var i3 = i2 - 1;

                var i4 = zOffset + j + 3;
                var i5 = zOffset + j + 2;
                var i6 = zOffset + j + 1;
                var i7 = zOffset + j;

                var z0 = z[i0];
                var z1 = z[i1];
                var z2 = z[i2];
                var z3 = z[i3];

                var z4 = z[i4];
                var z5 = z[i5];
                var z6 = z[i6];
                var z7 = z[i7];

                v += new Vector4(
                    h0 * (z0 + z4),
                    h0 * (z1 + z5),
                    h0 * (z2 + z6),
                    h0 * (z3 + z7));
            }

            v += TestTaps[hCenter] * new Vector4(
                z[zOffset + hCenter + 3],
                z[zOffset + hCenter + 2],
                z[zOffset + hCenter + 1],
                z[zOffset + hCenter + 0]);

            v.CopyTo(result.AsSpan(i));

            zOffset -= 4;

            if (zOffset < 0)
            {
                zOffset += hLength + 3;
            }
        }

        return result;
    }

    private float[] DoHalfBandHalfTapsInner(float[] source)
    {
        var result  = new float[source.Length];
        var h       = TestTaps;
        var hLength = h.Length;
        var z       = new float[h.Length * 2];
        var zOffset = 0;
        var hCenter = hLength / 2;

        for (var i = 0; i < source.Length; i++)
        {
            z[zOffset] = z[zOffset + hLength] = source[i];

            //PrintZLineIdx(z);
            //PrintZLineVal(z);
            //goto next;
            var sum = 0.0f;
            var tap = 0;
            for (; tap < hLength; tap += 8)
            {
                var tap0 = tap;
                var tap2 = tap + 2;
                var tap4 = tap + 4;
                var tap6 = tap + 6;
                sum += z[zOffset+hLength-1-tap0] * h[tap0];
                sum += z[zOffset+hLength-1-tap2] * h[tap2];
                sum += z[zOffset+hLength-1-tap4] * h[tap4];
                sum += z[zOffset+hLength-1-tap6] * h[tap6];
                continue;
                TestContext.WriteLine($"{tap0,2}, {tap2,2}, {tap4,2}, {tap6,2}");
            }
            for (; tap <= hCenter - (4 * 2 - 1); tap += 8)
            {
                break;
                // @formatter:off
                var h0Index = tap + 0; var h4Index = hLength - 1 - h0Index;
                var h1Index = tap + 2; var h5Index = hLength - 1 - h1Index;
                var h2Index = tap + 4; var h6Index = hLength - 1 - h2Index;
                var h3Index = tap + 6; var h7Index = hLength - 1 - h3Index;

                var h0 = h[h0Index]; var h4 = h[h4Index];
                var h1 = h[h1Index]; var h5 = h[h5Index];
                var h2 = h[h2Index]; var h6 = h[h6Index];
                var h3 = h[h3Index]; var h7 = h[h7Index];

                var z0Index = zOffset + tap;
                
                var z0 = z[z0Index];
                

                // @formatter:on
                sum += Vector4.Dot(new Vector4(h0, h1, h2, h3), new Vector4(z0));
                sum += Vector4.Dot(new Vector4(h4, h5, h6, h7), new Vector4(z0));

                TestContext.WriteLine(
                    $"loop 1 = zPos: {zOffset,2}, tap: {tap,2}, " +
                    $"h: {h0Index,2}, {h1Index,2}, {h2Index,2}, {h3Index,2} | {h4Index,2}, {h5Index,2}, {h6Index,2}, {h7Index,2}, " +
                    $"zIdx: {z0Index,2}, zVal: {z0,2}");
            }

            //TestContext.WriteLine("");
            for (; tap < hCenter; tap += 2) // this is good
            {
                var tap0 = tap;
                var tap1 = hLength - 1 - tap;
                sum +=
                    h[tap0] * z[zOffset + tap0] +
                    h[tap1] * z[zOffset + tap1]
                    ;
                continue;
                TestContext.WriteLine(
                    $"loop 2 = zPos: {zOffset,2}, tap: {tap0} | {tap1}");
            }

            Assert.That.IsGreaterThanOrEqual(hCenter, tap);

            sum += h[hCenter] * z[zOffset + hCenter];

            result[i] = sum;

            zOffset--;

            if (zOffset < 0)
            {
                zOffset += hLength;
            }
        }

        return result;
    }

    [Conditional("PRINT_Z_LINE_IDX")]
    private void PrintZLineIdx(float[] z)
    {
        TestContext.WriteLine($"Z idx: {string.Join(",", z.Select((_, t) => $"{t,2}"))}");
    }

    [Conditional("PRINT_Z_LINE_VAL")]
    private void PrintZLineVal(float[] z)
    {
        TestContext.WriteLine($"Z val: {string.Join(",", z.Select(s => $"{s,2}"))}");
    }
}