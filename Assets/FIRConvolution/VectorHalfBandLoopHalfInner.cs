using System;
using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class VectorHalfBandLoopHalfInner : Filter
    {
        public VectorHalfBandLoopHalfInner(IReadOnlyCollection<float> h)
            : base(h, 1)
        {
        }

        public override unsafe void Process(float* source, float* target, int length)
        {
            Filter filter = this;
            Filters.VectorHalfBandLoopHalfInner(source, target, length, ref filter);
        }
    }
}