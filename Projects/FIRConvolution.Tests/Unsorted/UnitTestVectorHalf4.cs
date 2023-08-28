using System.Diagnostics;
using System.Numerics;
using FIRConvolution.Tests.Formats.Audio.Extensions;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTestVectorHalf4
{
    [TestMethod]
    public void TestAllVersions()
    {
        const int taps = 23;

        Debug.WriteLine($"{nameof(taps)}: {taps}");

        Assert.AreEqual(1, taps % 2);

        const int center = taps / 2;

        Debug.WriteLine($"{nameof(center)}: {center}");

        Debug.WriteLine(string.Empty);
        Debug.WriteLine("normal");

        for (var i = 0; i < taps; i++)
        {
            Debug.Write($"{i,2}, ");
        }

        Debug.WriteLine(string.Empty);
        Debug.WriteLine("half");

        for (var i = 1; i < taps; i += 2)
        {
            Debug.Write($"{i,2}, ");
        }

        Debug.WriteLine(string.Empty);
        Debug.WriteLine("quarter V1");

        for (var i = 1; i < center; i += 2)
        {
            Debug.Write($"{i,2}, ");
        }

        Debug.WriteLine(string.Empty);
        Debug.WriteLine("quarter V2");

        for (var i = 1; i < center; i += 2)
        {
            Debug.Write($"{{{i,2}, {taps - i - 1,2}}}, ");
        }

        Debug.WriteLine(string.Empty);
        Debug.WriteLine("quarter V3");
        var tap = 1;
        int end;
        Debug.WriteLine("loop 1");
        for (end = center - 8; tap < end; tap += 8)
        {
            var tapL0 = tap + 0;
            var tapL1 = tap + 2;
            var tapL2 = tap + 4;
            var tapL3 = tap + 6;
            var tapR0 = taps - tapL0 - 1;
            var tapR1 = taps - tapL1 - 1;
            var tapR2 = taps - tapL2 - 1;
            var tapR3 = taps - tapL3 - 1;
            Debug.WriteLine($"{tapL0,2}, {tapL1,2}, {tapL2,2}, {tapL3,2}");
            Debug.WriteLine($"{tapR0,2}, {tapR1,2}, {tapR2,2}, {tapR3,2}");
        }

        Debug.WriteLine("loop 2");
        for (end = center - 4; tap < end; tap += 4)
        {
            var tapL0 = tap + 0;
            var tapL1 = tap + 2;
            var tapR0 = taps - tapL0 - 1;
            var tapR1 = taps - tapL1 - 1;
            Debug.WriteLine($"{tapL0,2}, {tapL1,2}");
            Debug.WriteLine($"{tapR0,2}, {tapR1,2}");
        }

        Debug.WriteLine("loop 3");
        for (end = center - 0; tap < end; tap += 2)
        {
            var tapL0 = tap + 0;
            var tapR0 = taps - tapL0 - 1;
            Debug.WriteLine($"{tapL0,2}");
            Debug.WriteLine($"{tapR0,2}");
        }

        static int FindNextMultiple(int input, int multiplier)
        {
            if (input == 0)
                return multiplier;

            return (input + multiplier - 1) / multiplier * multiplier;
        }

        for (var i = 0; i <= 8; i++)
        {
            var j = (i + 3) / 4 * 4;
            j = FindNextMultiple(-i, 4);
            Console.WriteLine($"{i}, {j}");
        }
    }

    [TestMethod]
    public void Test4LoopObsolete()
    {
        const int taps = 23;

        Debug.WriteLine($"{nameof(taps)}: {taps}");

        Assert.AreEqual(1, taps % 2);

        const int center = taps / 2;

        Debug.WriteLine($"{nameof(center)}: {center}");

        Debug.WriteLine(string.Empty);
        Debug.WriteLine("quarter V3");

        var tap = 1;

        int end;

        Debug.WriteLine("loop 1");

        for (end = center - 8; tap < end; tap += 8)
        {
            var tapL0 = tap + 0;
            var tapL1 = tap + 2;
            var tapL2 = tap + 4;
            var tapL3 = tap + 6;
            var tapR0 = taps - tapL0 - 1;
            var tapR1 = taps - tapL1 - 1;
            var tapR2 = taps - tapL2 - 1;
            var tapR3 = taps - tapL3 - 1;
            Debug.WriteLine($"{tapL0,2}, {tapL1,2}, {tapL2,2}, {tapL3,2}");
            Debug.WriteLine($"{tapR0,2}, {tapR1,2}, {tapR2,2}, {tapR3,2}");
        }

        Debug.WriteLine("loop 2");

        for (end = center - 4; tap < end; tap += 4)
        {
            var tapL0 = tap + 0;
            var tapL1 = tap + 2;
            var tapR0 = taps - tapL0 - 1;
            var tapR1 = taps - tapL1 - 1;
            Debug.WriteLine($"{tapL0,2}, {tapL1,2}");
            Debug.WriteLine($"{tapR0,2}, {tapR1,2}");
        }

        Debug.WriteLine("loop 3");

        for (end = center - 0; tap < end; tap += 2)
        {
            var tapL0 = tap + 0;
            var tapR0 = taps - tapL0 - 1;
            Debug.WriteLine($"{tapL0,2}");
            Debug.WriteLine($"{tapR0,2}");
        }
    }

    [TestMethod]
    public void Test1LoopVector4()
    {
        var taps  = FilterState.CreateHalfBand(44100, 8820).Coefficients;
        var nTaps = taps.Length;

        Console.WriteLine($"{nameof(nTaps)}: {nTaps}");

        Assert.AreEqual(1, nTaps % 2);

        var center = nTaps / 2;

        Console.WriteLine($"{nameof(center)}: {center}");

        var quarter = center / 2;

        Console.WriteLine($"{nameof(quarter)}: {quarter}");

        var length1 = (quarter + 3) / 4 * 4;

        Console.WriteLine($"{nameof(length1)}: {length1}");

        var length2 = length1 * 2 + 1;

        Console.WriteLine($"{nameof(length2)}: {length2}");

        var newTaps = new float[center];
        Array.Copy(taps, 0, newTaps, 0, newTaps.Length);

        Console.WriteLine("new taps:");
        for (var i = 0; i < newTaps.Length; i++)
        {
            Console.WriteLine($"{i,2}: {newTaps[i]}");
        }

        var newCenter = taps[center];
        Console.WriteLine($"new center: {newCenter}");

        Console.WriteLine();
        Console.WriteLine("old taps:");
        for (var i = 0; i < taps.Length; i++)
        {
            Console.WriteLine($"{i,2}: {taps[i]}");
        }

        Debug.WriteLine(string.Empty);
        Debug.WriteLine("quarter V3");

        var tap = 1;

        int end;

        Debug.WriteLine("loop 1");

        for (end = center - 8; tap < end; tap += 8)
        {
            var tapL0 = tap + 0;
            var tapL1 = tap + 2;
            var tapL2 = tap + 4;
            var tapL3 = tap + 6;
            var tapR0 = nTaps - tapL0 - 1;
            var tapR1 = nTaps - tapL1 - 1;
            var tapR2 = nTaps - tapL2 - 1;
            var tapR3 = nTaps - tapL3 - 1;
            Debug.WriteLine($"{tapL0,2}, {tapL1,2}, {tapL2,2}, {tapL3,2}");
            Debug.WriteLine($"{tapR0,2}, {tapR1,2}, {tapR2,2}, {tapR3,2}");
        }

        Debug.WriteLine("loop 2");

        for (end = center - 4; tap < end; tap += 4)
        {
            var tapL0 = tap + 0;
            var tapL1 = tap + 2;
            var tapR0 = nTaps - tapL0 - 1;
            var tapR1 = nTaps - tapL1 - 1;
            Debug.WriteLine($"{tapL0,2}, {tapL1,2}");
            Debug.WriteLine($"{tapR0,2}, {tapR1,2}");
        }

        Debug.WriteLine("loop 3");

        for (end = center - 0; tap < end; tap += 2)
        {
            var tapL0 = tap + 0;
            var tapR0 = nTaps - tapL0 - 1;
            Debug.WriteLine($"{tapL0,2}");
            Debug.WriteLine($"{tapR0,2}");
        }
    }

    [TestMethod]
    public void Test2LoopVector4()
    {
        var taps      = FilterState.CreateHalfBand(44100, 8820).Coefficients;
        var center    = taps.Length / 2;
        var quarter   = center / 2;
        var length1   = (quarter + 3) / 4 * 4;
        var length2   = length1 * 2 + 1;
        var newCenter = taps[center];
        var newTaps   = new float[length2];
        var delay     = new float[taps.Length * 2];
        var index     = 0;
        Array.Copy(taps, 0, newTaps, 0, newTaps.Length);

        Assert.AreEqual(1, taps.Length % 2);
        Debug.WriteLine($"{nameof(taps)}: {taps.Length}");
        Debug.WriteLine($"{nameof(center)}: {center}");
        Debug.WriteLine($"{nameof(quarter)}: {quarter}");
        Debug.WriteLine($"{nameof(length1)}: {length1}");
        Debug.WriteLine($"{nameof(length2)}: {length2}");

        if (true)
        {
            Debug.WriteLine($"new center: {newCenter}");
            Debug.WriteLine("new taps:");
            for (var i = 0; i < newTaps.Length; i++)
            {
                Debug.WriteLine($"{i,2}: {newTaps[i]}");
            }
        }

        if (false)
        {
            Debug.WriteLine("");
            Debug.WriteLine("old taps:");
            for (var i = 0; i < taps.Length; i++)
            {
                Debug.WriteLine($"{i,2}: {taps[i]}");
            }
        }

        Debug.WriteLine("");

        Span<float> sourceData = new float[1_000_000];
        Span<float> targetData = new float[sourceData.Length];

        const int chunkSize = 128;

        Span<float> source = new float[chunkSize];
        Span<float> target = new float[chunkSize];

        var chunks = (int)Math.Ceiling((float)sourceData.Length / source.Length);

        Debug.WriteLine($"{nameof(chunks)}: {chunks}");
        for (var chunk = 0; chunk < chunks; chunk++)
        {
            var start = chunk * chunkSize;
            var count = Math.Min(chunkSize, sourceData.Length - start);
            var input = sourceData.Slice(start, count);
            Debug.WriteLine($"{nameof(chunk)}: {chunk}, {nameof(start)}: {start}, {nameof(count)}: {count}, {nameof(index)}: {index}");
            source.Clear();
            target.Clear();
            input.CopyTo(source);
            for (var sample = 0; sample < chunkSize; sample++)
            {
                delay[index] = delay[index + taps.Length] = source[sample];

                var value = 0.0f;

                for (var tapIndex = 1; tapIndex < newTaps.Length; tapIndex += 8)
                {
                    var t0 = tapIndex + 0;
                    var t1 = tapIndex + 2;
                    var t2 = tapIndex + 4;
                    var t3 = tapIndex + 6;

                    var t4 = taps.Length - t0 - 1;
                    var t5 = taps.Length - t1 - 1;
                    var t6 = taps.Length - t2 - 1;
                    var t7 = taps.Length - t3 - 1;

                    var h0 = newTaps[t0];
                    var h1 = newTaps[t1];
                    var h2 = newTaps[t2];
                    var h3 = newTaps[t3];

                    var i0 = ((index - t0) % taps.Length + taps.Length) % taps.Length;
                    var i1 = ((index - t1) % taps.Length + taps.Length) % taps.Length;
                    var i2 = ((index - t2) % taps.Length + taps.Length) % taps.Length;
                    var i3 = ((index - t3) % taps.Length + taps.Length) % taps.Length;
                    var i4 = ((index - t4) % taps.Length + taps.Length) % taps.Length;
                    var i5 = ((index - t5) % taps.Length + taps.Length) % taps.Length;
                    var i6 = ((index - t6) % taps.Length + taps.Length) % taps.Length;
                    var i7 = ((index - t7) % taps.Length + taps.Length) % taps.Length;

                    var z0 = delay[i0];
                    var z1 = delay[i1];
                    var z2 = delay[i2];
                    var z3 = delay[i3];
                    var z4 = delay[i4];
                    var z5 = delay[i5];
                    var z6 = delay[i6];
                    var z7 = delay[i7];

                    const bool useVectors = true;

                    if (useVectors)
                    {
                        value += Vector4.Dot(new Vector4(h0, h1, h2, h3), new Vector4(z0, z1, z2, z3) + new Vector4(z4, z5, z6, z7));
                    }
                    else
                    {
                        value += h0 * (z0 + z4) + h1 * (z1 + z5) + h2 * (z2 + z6) + h3 * (z3 + z7);
                    }
                }

                value += newCenter * delay[((index - center) % taps.Length + taps.Length) % taps.Length];

                target[sample] = value;

                index--;

                if (index < 0)
                {
                    index += taps.Length;
                }
            }

            target[..count].CopyTo(targetData.Slice(start, count));
        }
    }
}