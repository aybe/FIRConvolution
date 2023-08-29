namespace FIRConvolution
{
    public static class FilterFactory
    {
        private static Filter Create
            (float[] h, int v, int t)
        {
            return new Filter(h, v); // TODO pass first tap
        }

        public static Filter CreateScalarFullBand
            (float[] h, int t)
        {
            return Create(h, 1, t);
        }

        public static Filter CreateScalarHalfBandLoopFull
            (float[] h, int t)
        {
            return Create(h, 1, t);
        }

        public static Filter CreateScalarHalfBandLoopHalf
            (float[] h, int t)
        {
            return Create(h, 1, t);
        }

        public static Filter CreateVectorFullBandInner
            (float[] h, int t)
        {
            return Create(h, 1, t);
        }

        public static Filter CreateVectorFullBandOuter
            (float[] h, int t)
        {
            return Create(h, 4, t);
        }

        public static Filter CreateVectorFullBandOuterInner
            (float[] h, int t)
        {
            return Create(h, 4, t);
        }

        public static Filter CreateVectorHalfBandLoopFullInner
            (float[] h, int t)
        {
            return Create(h, 1, t);
        }

        public static Filter CreateVectorHalfBandLoopFullOuter
            (float[] h, int t)
        {
            return Create(h, 4, t);
        }

        public static Filter CreateVectorHalfBandLoopFullOuterInner
            (float[] h, int t)
        {
            return Create(h, 4, t);
        }

        public static Filter CreateVectorHalfBandLoopHalfInner
            (float[] h, int t)
        {
            return Create(h, 1, t);
        }

        public static Filter CreateVectorHalfBandLoopHalfOuter
            (float[] h, int t)
        {
            return Create(h, 4, t);
        }

        public static Filter CreateVectorHalfBandLoopHalfOuterInner
            (float[] h, int t)
        {
            return Create(h, 4, t);
        }
    }
}