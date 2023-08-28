using System;
using System.Runtime.InteropServices;

namespace FIRConvolution.WorkInProgress
{
    public static unsafe class NativeMemoryFactory
    {
        public static T* Allocate<T>(T[] array) where T : unmanaged
        {
            var handle = Marshal.AllocHGlobal(array.Length * sizeof(T));

            var source = array.AsSpan();
            var target = new Span<T>((void*)handle, array.Length);

            source.CopyTo(target);

            return (T*)handle;
        }

        public static void Free<T>(T* handle) where T : unmanaged
        {
            if (handle == null)
            {
                return;
            }

            Marshal.FreeHGlobal((IntPtr)handle);
        }
    }
}