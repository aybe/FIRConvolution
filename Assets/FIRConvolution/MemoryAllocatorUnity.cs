using System;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace FIRConvolution
{
    [UsedImplicitly]
    public sealed class MemoryAllocatorUnity : MemoryAllocator<MemoryAllocatorUnity>
    {
        public override IntPtr AlignedAlloc(in int size, in int alignment)
        {
            AlignedAllocCheckArgs(size, alignment);

            unsafe
            {
                var pointer = UnsafeUtility.Malloc(size, alignment, Allocator.Persistent);

                if (pointer == null)
                {
                    throw new OutOfMemoryException();
                }

                return new IntPtr(pointer);
            }
        }

        public override void AlignedFree(in IntPtr pointer)
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

        public override IntPtr Alloc(in int size)
        {
            AllocCheckArgs(size);

            unsafe
            {
                var pointer = UnsafeUtility.Malloc(size, 1, Allocator.Persistent);

                if (pointer == null)
                {
                    throw new OutOfMemoryException();
                }

                return new IntPtr(pointer);
            }
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