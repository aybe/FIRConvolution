using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class VectorFullBandInner : Filter
    {
        public VectorFullBandInner(IReadOnlyCollection<float> h)
            : base(h, 1)
        {
        }

        public override unsafe void Process(float* source, float* target, int length)
        {
            Filter filter = this;
            Filters.VectorFullBandInner(source, target, length, ref filter);
        }
    }
}