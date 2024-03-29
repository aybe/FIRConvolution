﻿using JetBrains.Annotations;

namespace FIRConvolution.Samples.Tutorial
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum FFTSize
    {
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }
}