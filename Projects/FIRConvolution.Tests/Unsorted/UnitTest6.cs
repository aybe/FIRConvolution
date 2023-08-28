using FIRConvolution.Tests.Formats.Audio.Microsoft;

// ReSharper disable StringLiteralTypo

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTest6
{
    [TestMethod]
    public void SampleBySampleFull()
    {
        Process1(
            @"C:\temp\test-mono.wav",
            @"C:\temp\test-mono-fs-full.wav",
            FilterState1.CreateFull()
        );
    }

    [TestMethod]
    public void SampleBySampleHalf()
    {
        Process1(
            @"C:\temp\test-mono.wav",
            @"C:\temp\test-mono-fs-half.wav",
            FilterState1.CreateHalf()
        );
    }

    private static void Process1(string sourcePath, string targetPath, FilterState1 fs)
    {
        Console.WriteLine($"H: {fs.H.Length}");
        Console.WriteLine($"Z: {fs.Z.Length}");
        Console.WriteLine($"T: {fs.T.Length}");

        using var sourceStream = File.OpenRead(sourcePath);
        using var targetStream = File.Create(targetPath);
        using var sourceWav    = new Wav(sourceStream);
        using var targetWav    = new Wav(targetStream, sourceWav.Channels, sourceWav.BitsPerSample, sourceWav.SampleRate);

        const int count = 1024;

        var buffer = sourceWav.CreateBuffer<float>(count);

        int read;

        do
        {
            read = sourceWav.Read(buffer);

            for (var i = 0; i < read; i++)
            {
                buffer[i] = Convolve1(buffer[i], fs.H, fs.T, fs.Z, ref fs.P);
            }

            targetWav.Write(buffer, 0, count);
        } while (read == count);
    }

    private static float Convolve1(float sample, float[] h, int[] t, float[] z, ref int state)
    {
        var taps = h.Length;

        z[state] = z[state + taps] = sample;

        var result = 0.0f;

        foreach (var tap in t)
        {
            result += h[tap] * z[state + taps - tap];
        }

        state--;

        if (state < 0)
        {
            state += taps;
        }

        return result;
    }
}