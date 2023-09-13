// Problem 1: "ECall methods must be packaged into a system module."

// Problem 2: Microsoft Fakes does not support struct/extern static.

// Solution: global using, it isn't the first time it saved our ass!

global using ProfilerMarker = Unity.Profiling.Fakes.ShimProfilerMarker;
using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable CheckNamespace

namespace Unity.Profiling.Fakes;

internal readonly struct ShimProfilerMarker
{
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public ShimProfilerMarker(ProfilerCategory category, string name)
    {
    }

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public AutoScope Auto()
    {
        return new AutoScope();
    }

    public readonly struct AutoScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}