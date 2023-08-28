using System.Runtime.InteropServices;

namespace FIRConvolution.Tests.Unsorted;

[Serializable]
public unsafe struct NativeFilter : IDisposable
{
    public float* Coefficients;
    public int    CoefficientsLength;
    public float* DelayLine;
    public int    DelayLineLength;
    public int*   Taps;
    public int    TapsLength;
    public int    Position;

    public NativeFilter(Formats.Audio.Extensions.FilterState state)
    {
        var coefficients = state.Coefficients;
        var delayLine    = state.DelayLine;
        var taps         = state.Taps;

        Coefficients       = Alloc(coefficients);
        CoefficientsLength = coefficients.Length;
        DelayLine          = Alloc(delayLine);
        DelayLineLength    = delayLine.Length;
        Taps               = Alloc(taps);
        TapsLength         = taps.Length;
        Position           = 0;
    }

    public static T* Alloc<T>(T[] array) where T : unmanaged
    {
        var cb = array.Length * sizeof(T);

        var ptr = Marshal.AllocHGlobal(cb);

        var source = MemoryMarshal.AsBytes(array.AsSpan());

        var target = new Span<byte>((void*)ptr, cb);

        source.CopyTo(target);

        return (T*)ptr;
    }

    public static void Free<T>(T* ptr) where T : unmanaged
    {
        Marshal.FreeHGlobal((IntPtr)ptr);
    }

    public void Dispose()
    {
        Free(Coefficients);
        Free(DelayLine);
        Free(Taps);
    }
}