using System;

namespace FIRConvolution
{
    public static class MemoryAllocatorExtensions
    {
        public static unsafe T* AllocArray<T>(this MemoryAllocator allocator, T[] array)
            where T : unmanaged
        {
            var length = array.Length;

            var sizeOf = allocator.SizeOf<T>();

            var cb = sizeOf * length;

            var pointer = allocator.Alloc(cb);

            if (pointer == null)
            {
                throw new OutOfMemoryException();
            }

            var source = array.AsSpan();

            var target = new Span<T>(pointer, length);

            source.CopyTo(target);

            return (T*)pointer;
        }
    }
}