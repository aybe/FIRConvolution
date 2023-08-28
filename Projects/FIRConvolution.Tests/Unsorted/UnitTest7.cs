using System.Runtime.InteropServices;
using FIRConvolution.Tests.Formats.Audio.Microsoft;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTest7
{
    [TestMethod]
    public void ChunkByChunkFull()
    {
        Process2(
            @"C:\temp\messij-stereo.wav",
            @"C:\temp\messij-stereo-fs-full.wav",
            FilterState2.CreateFull()
        );
    }

    [TestMethod]
    public void ChunkByChunkHalf()
    {
        Process2(
            @"C:\temp\messij-stereo.wav",
            @"C:\temp\messij-stereo-fs-half.wav",
            FilterState2.CreateHalf()
        );
    }

    private static void Process2(string sourcePath, string targetPath, FilterState2 fs)
    {
        Console.WriteLine($"H: {fs.H.Length}");
        Console.WriteLine($"Z: {fs.Z.Length}");
        Console.WriteLine($"T: {fs.T.Length}");

        using var sourceStream = File.OpenRead(sourcePath);
        using var sourceWav    = new Wav(sourceStream);

        var channels = sourceWav.Channels;

        Assert.AreEqual(2, channels);

        using var targetStream = File.Create(targetPath);
        using var targetWav    = new Wav(targetStream, channels, sourceWav.BitsPerSample, sourceWav.SampleRate);

        const int count = 1024;

        var sourceBuffer1 = sourceWav.CreateBuffer<float>(count);
        var targetBuffer1 = sourceWav.CreateBuffer<float>(count);
        var sourceBuffer2 = MemoryMarshal.Cast<float, float2>(sourceBuffer1);
        var targetBuffer2 = MemoryMarshal.Cast<float, float2>(targetBuffer1);

        int read;

        do
        {
            read = sourceWav.Read(sourceBuffer1);

            Convolve2(sourceBuffer2, targetBuffer2, read, fs.H, fs.Z, fs.T, ref fs.P);

            targetWav.Write(targetBuffer1, 0, read);
        } while (read == count);
    }

    private static void Convolve2(
        Span<float2> source, Span<float2> target, int length, Span<float2> h, Span<float2> z, Span<int> t, ref int state)
    {
        var hLength = h.Length;
        var tLength = t.Length;

        for (var n = 0; n < length; n++)
        {
            z[state] = z[state + hLength] = source[n];

            var sample = new float2();

            for (var i = 0; i < tLength; i++)
            {
                var tap = t[i];

                sample += h[tap] * z[state + hLength - tap];
            }

            state--;

            if (state < 0)
            {
                state += hLength;
            }

            target[n] = sample;
        }
    }
}