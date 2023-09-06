using FIRConvolution.Tests.Formats.Audio.Extensions;
using FIRConvolution.Tests.Formats.Audio.Microsoft;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTest3
{
    [TestMethod]
    public void TestFilterRegular()
    {
        var filterH    = GetFilterCoefficients32();
        var filterZ    = new float[filterH.Length];
        var filterTaps = filterH.Length;

        using var srcStream = File.OpenRead(@"C:\temp\test-mono.wav");
        using var tgtStream = File.Create(@"C:\temp\test-mono-result-regular.wav");
        using var srcWav    = new Wav(srcStream);
        using var tgtWav    = new Wav(tgtStream, srcWav.Channels, srcWav.BitsPerSample, srcWav.SampleRate);

        int read;

        const int bufferLength = 1024;

        var buffer = srcWav.CreateBuffer<float>(bufferLength);

        do
        {
            read = srcWav.Read(buffer);

            var sampleIndex = 0;

            for (var i = 0; i < read; i++)
            {
                ref var sample = ref buffer[sampleIndex++];

                filterZ[0] = sample;

                var output = 0.0f;

                for (var k = 0; k < filterTaps; k++)
                {
                    output += filterH[k] * filterZ[k];
                }

                for (var k = filterTaps - 2; k >= 0; k--)
                {
                    filterZ[k + 1] = filterZ[k];
                }

                sample = output;
            }

            tgtWav.Write(buffer, 0, read);
        } while (read == bufferLength);
    }

    [TestMethod]
    public void TestFilterHalved()
    {
        var filterH       = GetFilterCoefficients32();
        var filterZ       = new float[filterH.Length];
        var filterTaps    = filterH.Length;
        var taps          = Formats.Audio.Extensions.Filter.HalfBandTaps(filterTaps);
        var filterIndices = taps;

        using var srcStream = File.OpenRead(@"C:\temp\test-mono.wav");
        using var tgtStream = File.Create(@"C:\temp\test-mono-result-halved.wav");
        using var srcWav    = new Wav(srcStream);
        using var tgtWav    = new Wav(tgtStream, srcWav.Channels, srcWav.BitsPerSample, srcWav.SampleRate);

        int read;

        const int bufferLength = 1024;

        var buffer = srcWav.CreateBuffer<float>(bufferLength);

        do
        {
            read = srcWav.Read(buffer);

            var sampleIndex = 0;

            for (var i = 0; i < read; i++)
            {
                ref var sample = ref buffer[sampleIndex++];

                filterZ[0] = sample;

                var output = 0.0f;

                foreach (var k in filterIndices)
                {
                    output += filterH[k] * filterZ[k];
                }

                for (var k = filterTaps - 2; k >= 0; k--)
                {
                    filterZ[k + 1] = filterZ[k];
                }

                sample = output;
            }

            tgtWav.Write(buffer, 0, read);
        } while (read == bufferLength);
    }


    [TestMethod]
    public void TestFilterHalvedDouble()
    {
        var filterH       = GetFilterCoefficients32();
        var filterZ       = new float[filterH.Length * 2];
        var filterTaps    = filterH.Length;
        var filterIndices = Enumerable.Range(0, filterTaps).Where(i => i % 2 == 1 || i == filterTaps / 2).ToArray();

        using var srcStream = File.OpenRead(@"C:\temp\test-mono.wav");
        using var tgtStream = File.Create(@"C:\temp\test-mono-result-halved-double.wav");
        using var srcWav    = new Wav(srcStream);
        using var tgtWav    = new Wav(tgtStream, srcWav.Channels, srcWav.BitsPerSample, srcWav.SampleRate);

        int read;

        const int bufferLength = 1024;

        var buffer = srcWav.CreateBuffer<float>(bufferLength);

        var zIndex = 0;
        do
        {
            read = srcWav.Read(buffer);

            var sampleIndex = 0;

            for (var i = 0; i < read; i++)
            {
                ref var sample = ref buffer[sampleIndex++];

                filterZ[zIndex]              = sample;
                filterZ[zIndex + filterTaps] = sample;

                var output = 0.0f;

                foreach (var k in filterIndices)
                {
                    output += filterH[k] * filterZ[zIndex + filterTaps - k];
                }

                sample = output;

                zIndex++;

                if (zIndex >= filterTaps)
                {
                    zIndex = 0;
                }
            }

            tgtWav.Write(buffer, 0, read);
        } while (read == bufferLength);
    }

    [TestMethod]
    public void TestFilterHalvedDoubleUnsafe()
    {
        using var srcStream = File.OpenRead(@"C:\temp\test-mono.wav");
        using var tgtStream = File.Create(@"C:\temp\test-mono-result-halved-double-unsafe.wav");
        using var srcWav    = new Wav(srcStream);
        using var tgtWav    = new Wav(tgtStream, srcWav.Channels, srcWav.BitsPerSample, srcWav.SampleRate);

        const int bufferLength = 1024;

        var state = Formats.Audio.Extensions.FilterState.CreateHalfBand();

        var buffer = srcWav.CreateBuffer<float>(bufferLength);

        unsafe
        {
            fixed (float* hArray = state.Coefficients)
            fixed (float* zArray = state.DelayLine)
            fixed (int* tArray = state.Taps)
            fixed (int* zState = &state.Position)
            {
                var hCount = state.Coefficients.Length;
                var tCount = state.Taps.Length;
                
                int read;

                do
                {
                    read = srcWav.Read(buffer);

                    var sampleIndex = 0;

                    for (var i = 0; i < read; i++)
                    {
                        ref var sample = ref buffer[sampleIndex++];

                        sample = Formats.Audio.Extensions.Filter.Convolve(sample, hArray, hCount, tArray, tCount, zArray, zState);
                    }

                    tgtWav.Write(buffer, 0, read);
                } while (read == bufferLength);
            }
        }
    }


    private static float[] GetFilterCoefficients32()
    {
        var coefficients64 = GetFilterCoefficients64();
        var coefficients32 = Array.ConvertAll(coefficients64, Convert.ToSingle);

        return coefficients32;
    }

    private static double[] GetFilterCoefficients64()
    {
        const int fs = 44100;
        const int fc = 11025;
        const int bw = 441;

        var coefficients = FilterUtility.LowPass(fs, fc, bw, FilterWindow.Blackman);

        return coefficients;
    }
}