using Unity.Collections.LowLevel.Unsafe;

namespace FIRConvolution
{
    public static class UnsafeUnity
    {
        public static int AlignOf<T>() where T : struct
        {
            return UnsafeUtility.AlignOf<T>();
        }

        public static bool IsBlittable<T>() where T : struct
        {
            return UnsafeUtility.IsBlittable<T>();
        }

        public static int SizeOf<T>() where T : struct
        {
            return UnsafeUtility.SizeOf<T>();
        }
    }
}