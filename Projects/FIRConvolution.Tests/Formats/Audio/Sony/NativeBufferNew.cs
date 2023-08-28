using System.Runtime.InteropServices;

namespace FIRConvolution.Tests.Formats.Audio.Sony;

public unsafe struct NativeBufferNew<T> : IDisposable where T : unmanaged
{
    public readonly T*  Array;
    public readonly int Count;
    public          int Index;

    public NativeBufferNew(int count)
    {
        var byteCount = (UIntPtr)(count * sizeof(T));
        var alignment = (UIntPtr)sizeof(T);
        Array = (T*)NativeMemory.AlignedAlloc(byteCount, alignment); // TODO
        Count = count;
        Index = 0;
        NativeMemory.Clear(Array, byteCount);
    }

    public ref T this[int index]
    {
        get
        {
            var n = Index + index;
            var m = Count;
            var i = (n % m + m) % m;

            if (Count % 2 == 0)
            {
                Assert.AreEqual(i, n & Count - 1); // TODO
            }

            return ref Array[i];
        }
    }

    public void Advance(int count = 2)
    {
        if (Count % 2 == 0)
        {
            Assert.AreEqual((Index + count) % Count, Index + count & Count - 2, $"{Index}, {count}, {Count}"); // TODO // BUG
        }

        Index = (Index + count) % Count;
    }

    public readonly void Dispose()
    {
        NativeMemory.AlignedFree(Array); // TODO
    }
}