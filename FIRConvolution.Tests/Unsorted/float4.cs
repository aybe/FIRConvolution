using System.Diagnostics.CodeAnalysis;

namespace FIRConvolution.Tests.Unsorted;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public struct float4
{
    public float x, y, z, w;

    public static readonly float4 zero = new();

    public float4(float xyzw) : this(xyzw, xyzw, xyzw, xyzw)
    {
    }

    public float4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public float2 xy => new(x, y);
    
    public float2 zw => new(z, w);

    public static float4 operator +(float4 a, float4 b)
    {
        return new float4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    }

    public static float4 operator *(float4 a, float4 b)
    {
        return new float4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
    }

    public static float4 operator *(float4 a, float b)
    {
        return new float4(a.x * b, a.y * b, a.z * b, a.w * b);
    }

    public readonly override string ToString()
    {
        return $"{nameof(x)}: {x}, {nameof(y)}: {y}, {nameof(z)}: {z}, {nameof(w)}: {w}";
    }
}