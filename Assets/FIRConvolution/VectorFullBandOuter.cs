using System;
using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class VectorFullBandOuter : Filter
    {
        public VectorFullBandOuter(IReadOnlyCollection<float> h)
            : base(h, 4)
        {
        }

        public override void Process(Span<float> source, Span<float> target, int length)
        {
            Filter filter = this;
            Filters.VectorFullBandOuter(source, target, length, ref filter);
        }
    }
}