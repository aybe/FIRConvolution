﻿using System;
using System.Collections.Generic;

namespace FIRConvolution
{
    public sealed class ScalarHalfBandLoopFull : Filter
    {
        public ScalarHalfBandLoopFull(IReadOnlyCollection<float> h)
            : base(h, 1)
        {
        }

        public override void Process(Span<float> source, Span<float> target, int length)
        {
            Filter filter = this;
            Filters.ScalarHalfBandLoopFull(source, target, length, ref filter);
        }
    }
}