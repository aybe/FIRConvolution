using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace FIRConvolution.Tests.Extensions
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class IEnumerableExtensions
    {
        public static T Single<T>(this IEnumerable source)
        {
            var array = source.OfType<T>().ToArray();

            if (array.Length != 1)
            {
                throw new InvalidOperationException($"Expecting a single object of type {typeof(T)}.");
            }

            var value = array.Single();

            return value;
        }
    }
}