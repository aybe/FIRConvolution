namespace FIRConvolution
{
    public static class FilterFactory
    {
        private static Filter Create
            (float[] h, int v)
        {
            return new Filter(h, v, Filters.TryGetHalfBandStartTap(h, out var result) ? result : default);
        }

        public static Filter CreateScalarFullBand
            (float[] h)
        {
            return Create(h, 1);
        }

        public static Filter CreateScalarHalfBandLoopFull
            (float[] h)
        {
            return Create(h, 1);
        }

        public static Filter CreateScalarHalfBandLoopHalf
            (float[] h)
        {
            return Create(h, 1);
        }

        public static Filter CreateVectorFullBandInner
            (float[] h)
        {
            return Create(h, 1);
        }

        public static Filter CreateVectorFullBandOuter
            (float[] h)
        {
            return Create(h, 4);
        }

        public static Filter CreateVectorFullBandOuterInner
            (float[] h)
        {
            return Create(h, 4);
        }

        public static Filter CreateVectorHalfBandLoopFullInner
            (float[] h)
        {
            return Create(h, 1);
        }

        public static Filter CreateVectorHalfBandLoopFullOuter
            (float[] h)
        {
            return Create(h, 4);
        }

        public static Filter CreateVectorHalfBandLoopFullOuterInner
            (float[] h)
        {
            return Create(h, 4);
        }

        public static Filter CreateVectorHalfBandLoopHalfInner
            (float[] h)
        {
            return Create(h, 1);
        }

        public static Filter CreateVectorHalfBandLoopHalfOuter
            (float[] h)
        {
            return Create(h, 4);
        }

        public static Filter CreateVectorHalfBandLoopHalfOuterInner
            (float[] h)
        {
            return Create(h, 4);
        }
    }
}