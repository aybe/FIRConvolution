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
            var pointer = UnsafeUtility.Malloc(cb, 1, Allocator.Persistent);

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
    }
}