using System.Runtime.InteropServices;
using FIRConvolution.Tests.Formats.Audio.Extensions;
using FIRConvolution.Tests.Formats.Audio.Microsoft;
using FIRConvolution.Tests.Formats.Audio.Sony;
using FIRConvolution.Tests.Unsorted;
using JetBrains.Annotations;

// ReSharper disable StringLiteralTypo
// ReSharper disable once CheckNamespace
namespace FIRConvolution.Tests.New;

[TestClass]
public class ZFilterTest
{
    private const int FilterTaps = 7;

    private static float[] FilterInput { get; } = Enumerable.Range(1, 32).Select(Convert.ToSingle).ToArray();

    public required TestContext TestContext { get; [UsedImplicitly] set; }

    [TestInitialize]
    public void TestInit()
    {
        ZFilter.Logger = o => TestContext.WriteLine(o?.ToString());
    }

    [TestCleanup]
    public void TestFree()
    {
        ZFilter.Logger = null;
    }

    [TestMethod]
    public void TestImpulseScalar()
    {
        unsafe
        {
            ProcessFilter(ZFilter.ProcessFilterScalar, 1, GetImpulse());
        }
    }

    [TestMethod]
    public void TestImpulseVector()
    {
        unsafe
        {
            ProcessFilter(ZFilter.ProcessFilterVector, 4, GetImpulse());
        }
    }

    [TestMethod]
    public unsafe void TestSoundVector()
    {
        const string sourcePath = @"C:\temp\input5s.wav";
        const string targetPath = @"C:\temp\input5s-filtered.wav";

        using var sourceStream = File.OpenRead(sourcePath);
        using var sourceWav    = new Wav(sourceStream);
        using var targetStream = File.Create(targetPath);
        using var targetWav    = new Wav(targetStream, sourceWav.Channels, sourceWav.BitsPerSample, sourceWav.SampleRate);

        const int bufferLength = 1024;

        var buffer = sourceWav.CreateBuffer<float>(bufferLength);

        var h = FilterState.CreateHalfBand().Coefficients;

        var filters = Enumerable.Range(0, sourceWav.Channels).Select(_ => new ZFilter(h, 4)).ToArray();

        // TODO ZFilter.Logger = null; // don't log else LUT will abandon at some point

        var size = 0;

        fixed (float* pBuffer = buffer)
        {
            int read;

            do
            {
                read = sourceWav.Read(buffer, 0, bufferLength);

                for (var i = 0; i < sourceWav.Channels; i++)
                {
                    ref var filter = ref filters[i];

                    ZFilter.ProcessFilterVector(pBuffer, pBuffer, read, sourceWav.Channels, i, ref filter);
                }

                targetWav.Write(buffer, 0, read);

                size += read;

                if (size >= 44100 * 30)
                {
                    break;
                }
            } while (read == bufferLength);
        }
    }

    [TestMethod]
    public unsafe void TestSoundVectorReverb()
    {
        const string sourcePath = @"C:\temp\input5s.wav";
        const string targetPath = @"C:\temp\input5s-reverb.wav";

        using var sourceStream = File.OpenRead(sourcePath);
        using var sourceWav    = new Wav(sourceStream);

        var channels = sourceWav.Channels;

        using var targetStream = File.Create(targetPath);
        using var targetWav    = new Wav(targetStream, channels, sourceWav.BitsPerSample, sourceWav.SampleRate);

        const int bufferLength = 1024;

        var sourceBuffer = sourceWav.CreateBuffer<float>(bufferLength);
        var filterBuffer = new float[sourceBuffer.Length];
        var outputBuffer = new float[sourceBuffer.Length];

        var h = FilterState.CreateHalfBand().Coefficients;

        var filters = Enumerable.Range(0, channels).Select(_ => new ZFilter(h, 4)).ToArray();

        var allocator = MemoryAllocatorNet.Instance;

        var rev       = new SpuReverbBurst(SpuReverbPreset.Hall, 44100, allocator);
        var revSource = MemoryMarshal.Cast<float, float2>(sourceBuffer);
        var revFilter = MemoryMarshal.Cast<float, float2>(filterBuffer);
        var revOutput = MemoryMarshal.Cast<float, float2>(outputBuffer);

        // ReSharper disable InlineTemporaryVariable
        // ReSharper disable UnusedVariable
        const float revBridge = 28000.0f / 32767.0f * 0.5f; // 0.4272591F
        const float revTunnel = 32000.0f / 32767.0f * 0.5f; // 0.4882962F
        const float revVolume = revTunnel;
        // ReSharper restore UnusedVariable
        // ReSharper restore InlineTemporaryVariable
        
        fixed (float* pSource = sourceBuffer)
        fixed (float* pFilter = filterBuffer)
        {
            int read;

            do
            {
                read = sourceWav.Read(sourceBuffer, 0, bufferLength);

                for (var i = 0; i < channels; i++)
                {
                    ref var filter = ref filters[i];

                    ZFilter.ProcessFilterVector(pSource, pFilter, read, channels, i, ref filter);
                }

                SpuReverbBurst.Process(revSource, revFilter, revOutput, read, 1.0f - revVolume, revVolume, ref rev);

                targetWav.Write(outputBuffer, 0, read);
            } while (read == bufferLength);
        }

        SpuReverbBurst.Free(ref rev, allocator);
    }

    private unsafe void ProcessFilter(ZFilterMethod filterMethod, int zVector, float[] h)
    {
        var filter = new ZFilter(h, zVector);
        var source = FilterInput;
        var target = new float[source.Length];

        fixed (float* pSource = source)
        fixed (float* pTarget = target)
        {
            filterMethod(pSource, pTarget, source.Length, 1, 0, ref filter);
        }

        TestContext.WriteLine("Impulse filtering result:");
        TestContext.WriteLine("Index, Expected, Actual, Deviation, Pass");

        var idx = 0;
        var err = 0;

        foreach (var (x, y) in source.Zip(target))
        {
            var abs = Math.Abs(x - y);

            var bad = abs > 0.0f;

            if (bad)
            {
                err++;
            }

            TestContext.WriteLine($"{idx,2}: {x,16}, {y,16}, {abs,16:P6}, {!bad}");

            idx++;
        }

        if (err is not 0)
        {
            Assert.Fail($"PASS: {source.Length - err}, FAIL: {err}");
        }
    }

    private static float[] GetImpulse()
    {
        var h = new float[FilterTaps];

        h[0] = 1.0f;
        return h;
    }
}