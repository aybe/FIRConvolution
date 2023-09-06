using System.Security.Cryptography;
using FIRConvolution.Tests.Formats.Audio.Extensions;
using FIRConvolution.Tests.Formats.Audio.Microsoft;
using FIRConvolution.Tests.Formats.Audio.Sony;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTest2
{
    [TestMethod]
    public void Test1()
    {
        Process(44100, 11025, 441, FilterWindow.Blackman, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }

    [TestMethod]
    public void Test2()
    {
        Process(44100, 11025, 882, FilterWindow.Blackman, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }


    [TestMethod]
    public void Test2a()
    {
        Process(44100, 11025, 1323, FilterWindow.Blackman, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }


    [TestMethod]
    public void Test2b()
    {
        Process(44100, 11025, 1764, FilterWindow.Blackman, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }

    [TestMethod]
    public void Test3()
    {
        Process(44100, 11025, 2205, FilterWindow.Blackman, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }

    [TestMethod]
    public void Test4()
    {
        Process(44100, 11025, 4410, FilterWindow.Blackman, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }

    [TestMethod]
    public void Test5()
    {
        Process(44100, 11025, 441, FilterWindow.Hamming, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }

    [TestMethod]
    public void Test6()
    {
        Process(44100, 11025, 882, FilterWindow.Hamming, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }

    [TestMethod]
    public void Test6a()
    {
        Process(44100, 11025, 1323, FilterWindow.Hamming, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }

    [TestMethod]
    public void Test6b()
    {
        Process(44100, 11025, 1764, FilterWindow.Hamming, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }

    [TestMethod]
    public void Test7()
    {
        Process(44100, 11025, 2205, FilterWindow.Hamming, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }

    [TestMethod]
    public void Test8()
    {
        Process(44100, 11025, 4410, FilterWindow.Hamming, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\short.wav");
    }

    [TestMethod]
    public void Test1234()
    {
        Process(44100, 11025, 441, FilterWindow.Blackman, @"C:\Users\aybe\source\repos\Wipeout.Private\psx reverb\mini.wav");
    }

    private static void Process(int fs, int fc, int bw, FilterWindow wt, string sourcePath)
    {
        var directory = Path.GetDirectoryName(sourcePath);
        var fileName  = Path.GetFileNameWithoutExtension(sourcePath);
        var extension = Path.GetExtension(sourcePath);

        var targetPath = Path.Combine(directory ?? string.Empty, Path.ChangeExtension($"{fileName}-{fs}-{fc}-{bw}-{wt}", extension));

        Console.WriteLine(targetPath);
        var coefficients = FilterUtility.LowPass(fs, fc, bw, wt);

        var filter = new Formats.Audio.Extensions.Filter(coefficients);

        Process(sourcePath, targetPath, filter);

        using var stream = File.OpenRead(targetPath);

        var hashData = SHA256.HashData(stream);
        var hash     = string.Concat(hashData.Select(s => s.ToString("x2")));

        Assert.AreEqual("9a2ef3aa7299fbb8973f63035202a04de9e8b9505731436c7f972c2304d1beac", hash);
    }

    private static void Process(string sourcePath, string targetPath, Formats.Audio.Extensions.Filter firFilter)
    {
        using var sourceStream = File.OpenRead(sourcePath);
        using var targetStream = File.Create(targetPath);

        using var sourceWav = new Wav(sourceStream);
        using var targetWav = new Wav(targetStream, sourceWav.Channels, sourceWav.BitsPerSample, sourceWav.SampleRate);

        const int samples = 1024;

        var sourceBuffer = sourceWav.CreateBuffer<float>(samples);
        var targetBuffer = sourceBuffer.ToArray();

        var factor = Converters.ToLinearVolume(0);

        var filter = new SpuReverb(SpuReverbPreset.Hall, (int)sourceWav.SampleRate);

        const float mixDry = 1.0f;
        const float mixWet = 1.0f;

        var filters = new[]
        {
            new Formats.Audio.Extensions.Filter(firFilter),
            new Formats.Audio.Extensions.Filter(firFilter)
        };

        int read;

        do
        {
            read = sourceWav.Read(sourceBuffer);

            for (var i = 0; i < read; i++)
            {
                var offsetL = i * 2 + 0;
                var offsetR = i * 2 + 1;

                var sourceL = sourceBuffer[offsetL];
                var sourceR = sourceBuffer[offsetR];

                var filterL = (float)filters[0].Process(sourceL);
                var filterR = (float)filters[1].Process(sourceR);

                filter.Process(filterL, filterR, out var targetL, out var targetR);

                targetBuffer[offsetL] = (0.5f * sourceL * mixDry + 0.5f * targetL * mixWet) * factor;
                targetBuffer[offsetR] = (0.5f * sourceR * mixDry + 0.5f * targetR * mixWet) * factor;
            }

            targetWav.Write(targetBuffer, 0, read);
        } while (read == samples);
    }
}