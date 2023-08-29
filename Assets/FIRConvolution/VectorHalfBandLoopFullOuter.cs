using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class VectorHalfBandLoopFullOuter
        : Filter
    {
        public VectorHalfBandLoopFullOuter(IReadOnlyCollection<float> h)
            : base(h, 4)
        {
        }

        public override unsafe void Process(float* source, float* target, int length)
        {
            Filter filter = this;
            Filters.VectorHalfBandLoopFullOuter(source, target, length, ref filter);
        }
    }
}