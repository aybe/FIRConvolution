using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace FIRConvolution
{
    public static class MemoryUtility
    {
        public static int AlignOf<T>() where T : struct
        {
            return AlignOfCache<T>.Value;
        }

        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        public static int AlignOf(Type type)
        {
            if (!IsBlittable(type))
            {
                throw new InvalidOperationException("The type is not blittable.");
            }

            var result = 1;

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                var fieldType = field.FieldType;

                if (fieldType.IsPrimitive == false)
                {
                    continue;
                }

                var sizeOf = Marshal.SizeOf(fieldType);

                result = Math.Max(result, sizeOf);
            }

            return result;
        }

        public static bool IsBlittable<T>()
        {
            var isBlittable = IsBlittableCache<T>.Value;

            return isBlittable;
        }

        private static bool IsBlittable(this Type type)
        {
            var handle = default(GCHandle);

            try
            {
                var instance = FormatterServices.GetUninitializedObject(type);

                handle = GCHandle.Alloc(instance, GCHandleType.Pinned);

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        private static class AlignOfCache<T>
        {
            public static readonly int Value = AlignOf(typeof(T));
        }

        private static class IsBlittableCache<T>
        {
            public static readonly bool Value = IsBlittable(typeof(T));
        }
    }
}