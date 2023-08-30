using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace FIRConvolution
{
    [UsedImplicitly]
    public sealed class MemoryAllocatorNet : MemoryAllocator<MemoryAllocatorNet>
    {
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