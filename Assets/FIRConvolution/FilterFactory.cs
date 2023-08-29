namespace FIRConvolution
{
    public static unsafe class FilterFactory
    {
        private static (Filter, FilterMethod) Create
            (float[] h, int v, int t, FilterMethod method)
        {
            return (new Filter(h, v), method); // TODO pass first tap
        }

        public static (Filter, FilterMethod) CreateScalarFullBand
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.ScalarFullBand);
        }

        public static (Filter, FilterMethod) CreateScalarHalfBandLoopFull
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.ScalarHalfBandLoopFull);
        }

        public static (Filter, FilterMethod) CreateScalarHalfBandLoopHalf
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.ScalarHalfBandLoopHalf);
        }

        public static (Filter, FilterMethod) CreateVectorFullBandInner
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.VectorFullBandInner);
        }

        public static (Filter, FilterMethod) CreateVectorFullBandOuter
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorFullBandOuter);
        }

        public static (Filter, FilterMethod) CreateVectorFullBandOuterInner
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorFullBandOuterInner);
        }

        public static (Filter, FilterMethod) CreateVectorHalfBandLoopFullInner
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.VectorHalfBandLoopFullInner);
        }

        public static (Filter, FilterMethod) CreateVectorHalfBandLoopFullOuter
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorHalfBandLoopFullOuter);
        }

        public static (Filter, FilterMethod) CreateVectorHalfBandLoopFullOuterInner
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorHalfBandLoopFullOuterInner);
        }

        public static (Filter, FilterMethod) CreateVectorHalfBandLoopHalfInner
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.VectorHalfBandLoopHalfInner);
        }

        public static (Filter, FilterMethod) CreateVectorHalfBandLoopHalfOuter
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorHalfBandLoopHalfOuter);
        }

        public static (Filter, FilterMethod) CreateVectorHalfBandLoopHalfOuterInner
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorHalfBandLoopHalfOuterInner);
        }
    }
}