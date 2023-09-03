using System;

namespace FIRConvolution
{
    public abstract class MemoryAllocator
    {
        public static MemoryAllocator Current
        {
            get
            {
#if UNITY
                return MemoryAllocatorUnity.Instance;
#else
                return MemoryAllocatorNet.Instance;
#endif
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