using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Mathematics;
using UnityEngine;

namespace FIRConvolution.Samples.Tutorial
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public static class FFTUtility
    {
        public static float DbToLinear(float dbValue)
        {
            var linear = math.pow(10.0f, dbValue / 20.0f);

            return linear;
        }

        public static float LinearToDb(float linearValue)
        {
            var db = math.log10(linearValue) * 20.0f;

            return db;
        }

        public static float BinToFrequency(int fftSize, float nyquistFrequency, int binIndex)
        {
            if (!Mathf.IsPowerOfTwo(fftSize))
            {
                throw new ArgumentOutOfRangeException(nameof(fftSize));
            }

            if (nyquistFrequency <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(nyquistFrequency));
            }

            if (binIndex < 0 || binIndex >= fftSize)
            {
                throw new ArgumentOutOfRangeException(nameof(binIndex));
            }

            var frequency = (float)binIndex / fftSize * nyquistFrequency;

            return frequency;
        }

        public static int FrequencyToBin(int fftSize, float nyquistFrequency, float frequency)
        {
            if (!Mathf.IsPowerOfTwo(fftSize))
            {
                throw new ArgumentOutOfRangeException(nameof(fftSize));
            }

            if (nyquistFrequency <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(nyquistFrequency));
            }

            if (frequency < 0.0f || frequency >= nyquistFrequency)
            {
                throw new ArgumentOutOfRangeException(nameof(frequency));
            }

            var bin = (int)Math.Round(frequency * fftSize / nyquistFrequency);

            return bin;
        }
    }
}