using System;
using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class ScalarFullBand : Filter
    {
        public ScalarFullBand(IReadOnlyCollection<float> h)
            : base(h, 1)
        {
        }

        public override void Process(float* source, float* target, int length)
        {
            Filter filter = this;
            Filters.ScalarFullBand(source, target, length, ref filter);
        }
    }
}