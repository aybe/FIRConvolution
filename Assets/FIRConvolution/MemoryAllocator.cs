using System;

namespace FIRConvolution
{
    public abstract class MemoryAllocator
    {
        public static MemoryAllocator Current
        public IntPtr AlignedAlloc<T>(T[] array) where T : unmanaged
        {
            get
            unsafe
            {
#if UNITY
                return MemoryAllocatorUnity.Instance;
#else
                return MemoryAllocatorNet.Instance;
#endif
                var length    = array.Length;
                var sizeOf    = SizeOf<T>();
                var size      = sizeOf * length;
                var alignment = AlignOf<T>();
                var pointer   = AlignedAlloc(size, alignment);

                var source = array.AsSpan();
                var target = new Span<T>(pointer.ToPointer(), length);

                source.CopyTo(target);

                return pointer;
            }
        }

        public abstract IntPtr AlignedAlloc(int cb, int alignment);

        public abstract void AlignedFree(IntPtr pointer);

        public abstract int AlignOf<T>() where T : unmanaged;

        public abstract unsafe void* Alloc(int cb);

        public abstract unsafe void Free(void* pointer);

        public abstract int SizeOf<TElement>() where TElement : unmanaged;
    }

    public abstract class MemoryAllocator<TInstance> : MemoryAllocator
        where TInstance : MemoryAllocator<TInstance>, new()
    {
        public static TInstance Instance { get; } = new();
    }
}