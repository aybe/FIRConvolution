using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class VectorFullBandOuterInner : Filter
    {
        public VectorFullBandOuterInner(IReadOnlyCollection<float> h)
            : base(h, 4)
        {
        }

        public override unsafe void Process(float* source, float* target, int length)
        {
            Filter filter = this;
            Filters.VectorFullBandOuterInner(source, target, length, ref filter);
        }
    }
}