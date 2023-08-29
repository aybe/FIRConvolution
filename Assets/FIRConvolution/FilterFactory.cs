namespace FIRConvolution
{
    public static unsafe class FilterFactory
    {
        private static FilterWorkspace Create
            (float[] h, int v, FilterMethod method)
        {
            return new FilterWorkspace(new Filter(h, v, Filter.TryGetHalfBandStartTap(h, out var result) ? result : default), method);
        }

        public static FilterWorkspace CreateScalarFullBand
            (float[] h)
        {
            return Create(h, 1, Filters.ScalarFullBand);
        }

        public static FilterWorkspace CreateScalarHalfBandLoopFull
            (float[] h)
        {
            return Create(h, 1, Filters.ScalarHalfBandLoopFull);
        }

        public static FilterWorkspace CreateScalarHalfBandLoopHalf
            (float[] h)
        {
            return Create(h, 1, Filters.ScalarHalfBandLoopHalf);
        }

        public static FilterWorkspace CreateVectorFullBandInner
            (float[] h)
        {
            return Create(h, 1, Filters.VectorFullBandInner);
        }

        public static FilterWorkspace CreateVectorFullBandOuter
            (float[] h)
        {
            return Create(h, 4, Filters.VectorFullBandOuter);
        }

        public static FilterWorkspace CreateVectorFullBandOuterInner
            (float[] h)
        {
            return Create(h, 4, Filters.VectorFullBandOuterInner);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopFullInner
            (float[] h)
        {
            return Create(h, 1, Filters.VectorHalfBandLoopFullInner);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopFullOuter
            (float[] h)
        {
            return Create(h, 4, Filters.VectorHalfBandLoopFullOuter);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopFullOuterInner
            (float[] h)
        {
            return Create(h, 4, Filters.VectorHalfBandLoopFullOuterInner);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopHalfInner
            (float[] h)
        {
            return Create(h, 1, Filters.VectorHalfBandLoopHalfInner);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopHalfOuter
            (float[] h)
        {
            return Create(h, 4, Filters.VectorHalfBandLoopHalfOuter);
        }

        public static FilterWorkspace CreateVectorHalfBandLoopHalfOuterInner
            (float[] h)
        {
            return Create(h, 4, Filters.VectorHalfBandLoopHalfOuterInner);
        }
    }
}