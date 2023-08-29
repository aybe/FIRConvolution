﻿using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class ScalarHalfBandLoopFull : Filter
    {
        public ScalarHalfBandLoopFull(IReadOnlyCollection<float> h)
            : base(h, 1)
        {
        }

        public override unsafe void Process(float* source, float* target, int length)
        {
            Filter filter = this;
            Filters.ScalarHalfBandLoopFull(source, target, length, ref filter);
        }
    }
}