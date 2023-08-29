namespace FIRConvolution
{
    public static unsafe class FilterFactory
    {
        private static FilterWorkspace Create
            (float[] h, int v, int t, FilterMethod method)
        {
            return new FilterWorkspace(new Filter(h, v), method); // TODO pass first tap
        }

        public static FilterWorkspace CreateScalarFullBand
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.ScalarFullBand);
        }

        public static FilterWorkspace CreateScalarHalfBandLoopFull
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.ScalarHalfBandLoopFull);
        }

        public static FilterWorkspace CreateScalarHalfBandLoopHalf
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.ScalarHalfBandLoopHalf);
        }

        public static FilterWorkspace CreateVectorFullBandInner
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.VectorFullBandInner);
        }

        public static FilterWorkspace CreateVectorFullBandOuter
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorFullBandOuter);
        }

        public static FilterWorkspace CreateVectorFullBandOuterInner
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorFullBandOuterInner);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopFullInner
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.VectorHalfBandLoopFullInner);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopFullOuter
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorHalfBandLoopFullOuter);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopFullOuterInner
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorHalfBandLoopFullOuterInner);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopHalfInner
            (float[] h, int t)
        {
            return Create(h, 1, t, Filters.VectorHalfBandLoopHalfInner);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopHalfOuter
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorHalfBandLoopHalfOuter);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopHalfOuterInner
            (float[] h, int t)
        {
            return Create(h, 4, t, Filters.VectorHalfBandLoopHalfOuterInner);
        }
    }
}