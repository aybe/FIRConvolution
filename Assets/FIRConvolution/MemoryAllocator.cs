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