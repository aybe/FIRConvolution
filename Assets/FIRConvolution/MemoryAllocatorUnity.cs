using System;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace FIRConvolution
{
    [UsedImplicitly]
    public sealed class MemoryAllocatorUnity : MemoryAllocator<MemoryAllocatorUnity>
    {
        public override  IntPtr AlignedAlloc(int cb, int alignment)
        {
            unsafe
            {
                var pointer = UnsafeUtility.Malloc(cb, alignment, Allocator.Persistent);

                if (pointer == null)
                {
                    throw new OutOfMemoryException();
                }

                return new IntPtr(pointer);
            }
        }

        public override void AlignedFree(IntPtr pointer)
        {
            unsafe
            {
                UnsafeUtility.Free(pointer.ToPointer(), Allocator.Persistent);
            }
        }

        public override int AlignOf<T>()
        {
            var alignOf = UnsafeUtility.AlignOf<T>();

            return alignOf;
        }

        public override unsafe void* Alloc(int cb)
        {
            var pointer = UnsafeUtility.Malloc(cb, 1, Allocator.Persistent); // TODO alignment

            if (pointer == null)
            {
                throw new OutOfMemoryException();
            }

            return pointer;
        }

        public override unsafe void Free(void* pointer)
        {
            UnsafeUtility.Free(pointer, Allocator.Persistent);
        }

        public override int SizeOf<T>()
        {
            var sizeOf = UnsafeUtility.SizeOf<T>();

            return sizeOf;
        }

        private static unsafe T* AlignedAlloc<T>(T[] source) where T : unmanaged // TODO
        {
            var length    = source.Length;
            var sizeOf    = UnsafeUtility.SizeOf<T>();
            var size      = sizeOf * length;
            var alignment = UnsafeUtility.AlignOf<T>();
            var pointer   = UnsafeUtility.Malloc(size, alignment, Allocator.Persistent);

            if (pointer == null)
            {
                throw new OutOfMemoryException();
            }

            var span = new Span<T>(pointer, length);

            source.AsSpan().CopyTo(span);

            return (T*)pointer;
        }
    }
}