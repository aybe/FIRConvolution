using FIRConvolution.Tests.Unsorted;

namespace FIRConvolution.Tests.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<float2> ToFloat2(this IEnumerable<float> source)
    {
        return source.Select(s => new float2(s, s));
    }
}