using System.Runtime.InteropServices;
using FIRConvolution.Tests.Formats.Audio.Microsoft;
using FIRConvolution.Tests.Formats.Audio.Sony;

// ReSharper disable StringLiteralTypo

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTest9
{
    [TestMethod]
    public void ReverbChunkByChunkFull32()
    {
        Process3(
            @"C:\temp\messij-stereo.wav",
            @"C:\temp\messij-stereo-fs-full-reverb-final-32.wav",
            FilterState2.CreateFull()
        );
    }

    [TestMethod]
    public void ReverbChunkByChunkHalf32()
    {
        Process3(
            @"C:\temp\messij-stereo.wav",
            @"C:\temp\messij-stereo-fs-half-reverb-final-32.wav",
            FilterState2.CreateHalf()
        );
    }

    [TestMethod]
    public void ReverbChunkByChunkFull16()
    {
        Process4(
            @"C:\temp\messij-stereo.wav",
            @"C:\temp\messij-stereo-fs-full-reverb-final-16.wav",
            FilterState2.CreateFull()
        );
    }

    [TestMethod]
    public void ReverbChunkByChunkHalf16()
    {
        Process4(
            @"C:\temp\messij-stereo.wav",
            @"C:\temp\messij-stereo-fs-half-reverb-final16.wav",
            FilterState2.CreateHalf()
        );
    }

    private static unsafe void Process3(string sourcePath, string targetPath, FilterState2 fs)
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
        var reverbBuffer1 = sourceWav.CreateBuffer<float>(count);
        var sourceBuffer2 = MemoryMarshal.Cast<float, float2>(sourceBuffer1);
        var targetBuffer2 = MemoryMarshal.Cast<float, float2>(targetBuffer1);
        var reverbBuffer2 = MemoryMarshal.Cast<float, float2>(reverbBuffer1);

        var settings = new SpuReverbSettings(SpuReverbPreset.Hall, 44100);
        var buffer   = new NativeBufferNew<float>(262144);

        fixed (float2* pSource = sourceBuffer2)
        fixed (float2* pTarget = targetBuffer2)
        fixed (float2* h = fs.H)
        fixed (float2* z = fs.Z)
        fixed (int* t = fs.T)
        {
            int read;

            do
            {
                read = sourceWav.Read(sourceBuffer1);

                DoFilter(pSource, pTarget, read, h, fs.H.Length, t, fs.T.Length, z, ref fs.P);

                SpuReverbBurst.Process(targetBuffer2, reverbBuffer2, read, ref settings, ref buffer);

                for (var i = 0; i < read; i++)
                {
                    var dry = sourceBuffer2[i];
                    var wet = reverbBuffer2[i];

                    targetBuffer2[i] = wet * 0.5f + dry * 0.5f;
                }

                targetWav.Write(targetBuffer1, 0, read);
            } while (read == count);
        }
    }

    private static unsafe void Process4(string sourcePath, string targetPath, FilterState2 fs)
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
        var reverbBuffer1 = sourceWav.CreateBuffer<float>(count);
        var sourceBuffer2 = MemoryMarshal.Cast<float, float2>(sourceBuffer1);
        var targetBuffer2 = MemoryMarshal.Cast<float, float2>(targetBuffer1);
        var reverbBuffer2 = MemoryMarshal.Cast<float, float2>(reverbBuffer1);

        var settings = new SpuReverbSettings(SpuReverbPreset.Hall, 44100);
        var buffer   = new NativeBufferNew<float>(262144);

        var rv = new SpuReverbFilter16(SpuReverbPreset.Hall);

        fixed (float2* pSource = sourceBuffer2)
        fixed (float2* pTarget = targetBuffer2)
        fixed (float2* h = fs.H)
        fixed (float2* z = fs.Z)
        fixed (int* t = fs.T)
        {
            int read;

            do
            {
                read = sourceWav.Read(sourceBuffer1);

                DoFilter(pSource, pTarget, read, h, fs.H.Length, t, fs.T.Length, z, ref fs.P);

                // SpuReverbBurst.Process(targetBuffer2, reverbBuffer2, read, ref settings, ref buffer);

                for (var i = 0; i < read; i++)
                {
                    var s = targetBuffer2[i];
                    var l = (short)(s.x * 32767);
                    var r = (short)(s.y * 32767);
                    rv.Process(l, r, out var u, out var v);
                    reverbBuffer2[i] = new float2(u / 32767.0f, v / 32767.0f);
                }

                for (var i = 0; i < read; i++)
                {
                    var dry = sourceBuffer2[i];
                    var wet = reverbBuffer2[i];

                    targetBuffer2[i] = wet * 0.5f + dry * 0.5f;
                }

                targetWav.Write(targetBuffer1, 0, read);
            } while (read == count);
        }
    }

    private static unsafe void DoFilter(
        float2* src, float2* tgt, int len, float2* h, int hLen, int* t, int tLen, float2* z, ref int pos)
    {
        for (var n = 0; n < len; n++)
        {
            z[pos] = z[pos + hLen] = src[n];

            var val = float2.zero;

            for (var i = 0; i < tLen; i++)
            {
                var tap = t[i];

                val += h[tap] * z[pos + hLen - tap];
            }

            pos--;

            if (pos < 0)
            {
                pos += hLen;
            }

            tgt[n] = val;
        }
    }
}