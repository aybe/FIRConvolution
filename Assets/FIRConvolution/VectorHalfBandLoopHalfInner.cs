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

        public override void Process(Span<float> source, Span<float> target, int length)
        {
            Filter filter = this;
            Filters.VectorHalfBandLoopHalfInner(source, target, length, ref filter);
        }
    }
}