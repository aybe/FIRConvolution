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
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestScalarFull()
        {
            TestFilter(FilterType.ScalarFull);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestScalarHalfFull()
        {
            TestFilter(FilterType.ScalarHalfFull);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestScalarHalfHalf()
        {
            TestFilter(FilterType.ScalarHalfHalf);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestVectorFullInner()
        {
            TestFilter(FilterType.VectorFullInner);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestVectorFullOuter()
        {
            TestFilter(FilterType.VectorFullOuter);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestVectorFullOuterInner()
        {
            TestFilter(FilterType.VectorFullOuterInner);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestVectorHalfFullInner()
        {
            TestFilter(FilterType.VectorHalfFullInner);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestVectorHalfFullOuter()
        {
            TestFilter(FilterType.VectorHalfFullOuter);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestVectorHalfFullOuterInner()
        {
            TestFilter(FilterType.VectorHalfFullOuterInner);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestVectorHalfHalfInner()
        {
            TestFilter(FilterType.VectorHalfHalfInner);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestVectorHalfHalfOuter()
        {
            TestFilter(FilterType.VectorHalfHalfOuter);
        }

        [Test]
        [Category("SkipWhenLiveUnitTesting")]
        [Performance]
        public void TestVectorHalfHalfOuterInner()
        {
            TestFilter(FilterType.VectorHalfHalfOuterInner);
        }
    }
}