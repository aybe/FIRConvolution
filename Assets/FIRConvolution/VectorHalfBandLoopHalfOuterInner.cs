using System;
using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class VectorHalfBandLoopHalfOuterInner : Filter
    {
        public VectorHalfBandLoopHalfOuterInner(IReadOnlyCollection<float> h)
            : base(h, 4)
        {
        }

        public override unsafe void Process(float* source, float* target, int length)
        {
            Filter filter = this;
            Filters.VectorHalfBandLoopHalfOuterInner(source, target, length, ref filter);
        }
    }
}