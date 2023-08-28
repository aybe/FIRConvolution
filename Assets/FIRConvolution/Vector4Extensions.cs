using System;
using System.Numerics;

namespace FIRConvolution
{
    public static class Vector4Extensions
    {
        public static void CopyTo(this Vector4 target, Span<float> span)
        {
            span[0] = target.X;
            span[1] = target.Y;
            span[2] = target.Z;
            span[3] = target.W;
        }
    }
}