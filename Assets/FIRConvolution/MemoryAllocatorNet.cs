using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace FIRConvolution
{
    [UsedImplicitly]
    public sealed class MemoryAllocatorNet : MemoryAllocator<MemoryAllocatorNet>
    {
        public override IntPtr AlignedAlloc(in int size, in int alignment)
        {
            AlignedAllocCheckArgs(size, alignment);

            var padding = alignment - 1 + IntPtr.Size;
            var initial = Marshal.AllocHGlobal(size + padding);
            var aligned = (IntPtr)((long)initial + padding & ~(alignment - 1));
            var storage = aligned - IntPtr.Size;

            Marshal.WriteIntPtr(storage, initial);

            return aligned;
        }

        public override void AlignedFree(in IntPtr pointer)
        {
            var storage = pointer - IntPtr.Size;
            var initial = Marshal.ReadIntPtr(storage);

            Marshal.FreeHGlobal(initial);
        }

        public override int AlignOf<T>()
        {
            var alignOf = MemoryUtility.AlignOf<T>();

            return alignOf;
        }

        public override unsafe void* Alloc(int cb)
        {
            var pointer = Marshal.AllocHGlobal(cb);

            if (pointer == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            return (void*)pointer;
        }

        public override unsafe void Free(void* pointer)
        {
            Marshal.FreeHGlobal((IntPtr)pointer);
        }

        public override int SizeOf<T>()
        {
            var sizeOf = Marshal.SizeOf<T>();

            return sizeOf;
        }
    }
}