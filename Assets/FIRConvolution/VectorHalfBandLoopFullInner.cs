using System;
using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class VectorHalfBandLoopFullInner
        : Filter
    {
        public VectorHalfBandLoopFullInner(IReadOnlyCollection<float> h)
            : base(h, 1)
        {
        }

        public override void Process(float* source, float* target, int length)
        {
            Filter filter = this;
            Filters.VectorHalfBandLoopFullInner(source, target, length, ref filter);
        }
    }
}