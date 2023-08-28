using System;
using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class VectorFullBandInner : Filter
    {
        public VectorFullBandInner(IReadOnlyCollection<float> h)
            : base(h, 1)
        {
        }

        public override void Process(Span<float> source, Span<float> target, int length)
        {
            Filter filter = this;
            Filters.VectorFullBandInner(source, target, length, ref filter);
        }
    }
}