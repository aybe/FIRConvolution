using System;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace FIRConvolution.Tests
{
    public sealed class UnitTestFilters
    {
        // TODO this currently takes ~108.538s to execute, editor appears frozen but it isn't

        private const int WarmupCount = 10;

        private const int MeasurementCount = 10;

        private const int IterationsPerMeasurement = 100;

        private static unsafe void TestFilter(FilterType filterType)
        {
            filterType.GetHandlers(out var create, out var handler);

            var lp64 = FilterUtility.LowPass(44100.0d, 11025.0d, 441.0d, FilterWindow.Blackman);
            var lp32 = Array.ConvertAll(lp64, Convert.ToSingle);

            var allocator = MemoryAllocatorUnity.Instance;

            var filter = create(lp32, allocator);

            // TODO create a method in Filter for this
            allocator.Free(new IntPtr(filter.H));
            allocator.Free(new IntPtr(filter.Z));

            const int length = 1024;

            var source = stackalloc float[length];
            var target = stackalloc float[length];

            Measure
                .Method(Action)
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .IterationsPerMeasurement(IterationsPerMeasurement)
                .GC()
                .Run();

            return;

            void Action()
            {
                handler(source, target, length, 1, 0, ref filter);
            }
        }

        [Test]
        [Performance]
        public void TestScalarFullBand()
        {
            TestFilter(FilterType.ScalarFullBand);
        }

        [Test]
        [Performance]
        public void TestScalarHalfBandFullLoop()
        {
            TestFilter(FilterType.ScalarHalfBandFullLoop);
        }

        [Test]
        [Performance]
        public void TestScalarHalfBandHalfLoop()
        {
            TestFilter(FilterType.ScalarHalfBandHalfLoop);
        }

        [Test]
        [Performance]
        public void TestVectorFullBandInner()
        {
            TestFilter(FilterType.VectorFullBandInner);
        }

        [Test]
        [Performance]
        public void TestVectorFullBandOuter()
        {
            TestFilter(FilterType.VectorFullBandOuter);
        }

        [Test]
        [Performance]
        public void TestVectorFullBandOuterInner()
        {
            TestFilter(FilterType.VectorFullBandOuterInner);
        }

        [Test]
        [Performance]
        public void TestVectorHalfBandFullLoopInner()
        {
            TestFilter(FilterType.VectorHalfBandFullLoopInner);
        }

        [Test]
        [Performance]
        public void TestVectorHalfBandFullLoopOuter()
        {
            TestFilter(FilterType.VectorHalfBandFullLoopOuter);
        }

        [Test]
        [Performance]
        public void TestVectorHalfBandFullLoopOuterInner()
        {
            TestFilter(FilterType.VectorHalfBandFullLoopOuterInner);
        }

        [Test]
        [Performance]
        public void TestVectorHalfBandHalfLoopInner()
        {
            TestFilter(FilterType.VectorHalfBandHalfLoopInner);
        }

        [Test]
        [Performance]
        public void TestVectorHalfBandHalfLoopOuter()
        {
            TestFilter(FilterType.VectorHalfBandHalfLoopOuter);
        }

        [Test]
        [Performance]
        public void TestVectorHalfBandHalfLoopOuterInner()
        {
            TestFilter(FilterType.VectorHalfBandHalfLoopOuterInner);
        }
    }
}