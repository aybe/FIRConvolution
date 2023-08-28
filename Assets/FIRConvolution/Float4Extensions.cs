using System;
using Unity.Mathematics;

namespace FIRConvolution
{
    public static class Float4Extensions
    {
        internal static void CopyTo(this in float4 source, in Span<float> target)
        {
            if (target.Length < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(target));
            }

            target[0] = source[0];
            target[1] = source[1];
            target[2] = source[2];
            target[3] = source[3];
        }
    }
}