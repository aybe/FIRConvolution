// ReSharper disable IdentifierTypo

namespace FIRConvolution.Tests.Formats.Audio.Extensions
{
    /// <summary>
    ///     https://fiiir.com/
    /// </summary>
    public sealed class Filter
    {
        public Filter(Filter filter)
            : this(filter.Coefficients)
        {
        }

        public Filter(IReadOnlyCollection<double> coefficients)
        {
            Coefficients = coefficients.ToArray();
            DelayLine    = new double[coefficients.Count];
            Position     = 0;
        }

        private double[] Coefficients { get; }

        private double[] DelayLine { get; }

        private int Position { get; set; }

        public void Clear()
        {
            Array.Clear(DelayLine, 0, DelayLine.Length);

            Position = 0;
        }

        public double Process(double sample)
        {
            DelayLine[Position] = sample;

            var result = 0.0d;
            var offset = Position;
            var source = Coefficients;
            var length = source.Length;

            for (var i = 0; i < length; i++)
            {
                var input = source[i];
                var delay = DelayLine[offset];
                var value = input * delay;

                result = result + value;

                offset--;

                if (offset < 0)
                {
                    offset = length - 1;
                }
            }

            Position++;

            if (Position >= length)
            {
                Position = 0;
            }

            return result;
        }

        /// <summary>
        ///     Performs a convolution.
        /// </summary>
        /// <param name="sample">Input sample.</param>
        /// <param name="hArray">Coefficients.</param>
        /// <param name="hCount">Coefficients count.</param>
        /// <param name="tArray">Taps.</param>
        /// <param name="tCount">Taps count.</param>
        /// <param name="zArray">Doubled delay line.</param>
        /// <param name="zState">Doubled delay line state.</param>
        /// <returns>
        ///     The resulting filtered sample.
        /// </returns>
        public static unsafe float Convolve(
            in float sample, in float* hArray, in int hCount, in int* tArray, in int tCount, in float* zArray, in int* zState)
        {
            var index1 = *zState;
            var index2 = *zState + hCount;

            zArray[index1] = sample;
            zArray[index2] = sample;

            var filter = 0.0f;

            for (var pos = 0; pos < tCount; pos++)
            {
                var tap = tArray[pos];

                filter += hArray[tap] * zArray[index2 - tap];
            }

            index1++;

            if (index1 >= hCount)
            {
                index1 = 0;
            }

            *zState = index1;

            return filter;
        }

        public static int[] HalfBandTaps(int count)
        {
            var taps = Enumerable.Range(0, count).Where(i => i % 2 == 1 || i == count / 2).ToArray();

            return taps;
        }
    }
}