using JetBrains.Annotations;

namespace FIRConvolution.Tests.Unsorted;

[Serializable]
[NoReorder]
public sealed class NativeFilterState : IDisposable
{
    public NativeBufferF32 Coefficients;
    public NativeBufferF32 DelayLine;
    public NativeBufferI32 Taps;
    public int             Position;

    public void Dispose()
    {
    }
}