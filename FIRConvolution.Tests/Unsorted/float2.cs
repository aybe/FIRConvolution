using System.Diagnostics.CodeAnalysis;

namespace FIRConvolution.Tests.Unsorted;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public struct float2
{
    public float x, y;

    public static readonly float2 zero = new();

    public float2(float xy) : this(xy, xy)
    {
    }

    public float2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static float2 operator +(float2 a, float2 b)
    {
        return new float2(a.x + b.x, a.y + b.y);
    }

    public static float2 operator *(float2 a, float2 b)
    {
        return new float2(a.x * b.x, a.y * b.y);
    }

    public static float2 operator *(float2 a, float b)
    {
        return new float2(a.x * b, a.y * b);
    }

    public readonly override string ToString()
    {
        return $"{nameof(x)}: {x}, {nameof(y)}: {y}";
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class math
{
    public static float csum(float2 x)
    {
        return x.x + x.y;
    }
}