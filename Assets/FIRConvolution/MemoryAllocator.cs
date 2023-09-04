using System;
using JetBrains.Annotations;

namespace FIRConvolution
{
    public abstract class MemoryAllocator
    {
        public IntPtr AlignedAlloc<T>(T[] array) where T : unmanaged
        {
            unsafe
            {
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

        public abstract IntPtr AlignedAlloc(in int size, in int alignment);

        [AssertionMethod]
        protected static void AlignedAllocCheckArgs(in int size, in int alignment)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size,
                    "The size must not be less than zero.");
            }

            if (alignment <= 0 || (alignment & alignment - 1) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(alignment), alignment,
                    "The alignment must be a power of two.");
            }
        }

        public abstract void AlignedFree(in IntPtr pointer);

        public abstract int AlignOf<T>() where T : unmanaged;

        public abstract IntPtr Alloc(in int size);

        [AssertionMethod]
        protected static void AllocCheckArgs(in int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size,
                    "The size must not be less than zero.");
            }
        }

        public abstract void Free(in IntPtr pointer);

        public abstract int SizeOf<T>() where T : unmanaged;
    }

    public abstract class MemoryAllocator<TInstance> : MemoryAllocator
        where TInstance : MemoryAllocator<TInstance>, new()
    {
        public static TInstance Instance { get; } = new();
    }
}