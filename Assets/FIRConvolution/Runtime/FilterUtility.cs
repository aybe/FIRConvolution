using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FIRConvolution
{
    public static class FilterUtility
    {
        public static double[] LowPass(double fs, double fc, double bw, FilterWindow wt)
        {
            if (fs < 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(fs), fc, null);
            }

            if (fc >= fs * 0.5)
            {
                throw new ArgumentOutOfRangeException(nameof(fc), fc, null);
            }

            if (bw < fs * 0.01 || bw > fs * 0.49)
            {
                throw new ArgumentOutOfRangeException(nameof(bw));
            }

            var k = wt switch
            {
                FilterWindow.Blackman => 4.6,
                FilterWindow.Hamming  => 3.1,
                _                     => throw new ArgumentOutOfRangeException(nameof(wt), wt, null)
            };

            Func<int, int, double> w = wt switch
            {
                FilterWindow.Blackman => Blackman,
                FilterWindow.Hamming  => Hamming,
                _                     => throw new ArgumentOutOfRangeException(nameof(wt), wt, null)
            };

            var f = fc / fs;
            var b = bw / fs;
            var n = (int)Math.Ceiling(k / b);

            if (n % 2 == 0)
            {
                n++;
            }

            var h = new double[n];

            for (var i = 0; i < n; i++)
            {
                var d = Sinc(n, i, f);
                var e = w(n, i);
                var g = d * e;
                h[i] = g;
            }

            var sum = h.Sum();

            for (var i = 0; i < n; i++)
            {
                h[i] /= sum;
            }

            return h;
        }

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private static double Sinc(int n, int i, double d)
        {
            if (i == n / 2)
            {
                return 1;
            }

            var x = Math.PI * (2.0 * d * (i - (n - 1) / 2.0));
            var y = Math.Sin(x);
            var z = y / x;

            return z;
        }

        private static double Blackman(int n, int i)
        {
            const double a0 = 0.42;
            const double a1 = 0.50;
            const double a2 = 0.08;

            var w = a0 -
                    a1 * Math.Cos(2.0 * Math.PI * i / (n - 1)) +
                    a2 * Math.Cos(4.0 * Math.PI * i / (n - 1));

            return w;
        }

        private static double Hamming(int n, int i)
        {
            var x = Math.Cos(2.0 * Math.PI * i / (n - 1));
            var w = 0.54 - 0.46 * x;

            return w;
        }
    }
}