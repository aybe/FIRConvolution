using System.Text;
using System.Text.RegularExpressions;

namespace FIRConvolution.Tests.Extensions;

public static class TypeExtensions
{
    private const RegexOptions GetRealTypeNameRegexOptions = RegexOptions.Compiled | RegexOptions.Singleline;

    private static readonly Regex GetRealTypeNameRegex1 = new(@"`\d\[", GetRealTypeNameRegexOptions);

    private static readonly Regex GetRealTypeNameRegex2 = new(@"\]", GetRealTypeNameRegexOptions);

    private static readonly Regex GetRealTypeNameRegex3 = new(@"\w+\.", GetRealTypeNameRegexOptions);

    public static string GetRealTypeName(this Type type, bool full)
    {
        var name = type.ToString();

        name = GetRealTypeNameRegex1.Replace(name, "<");

        name = GetRealTypeNameRegex2.Replace(name, ">");

        if (full)
        {
            return name;
        }

        name = GetRealTypeNameRegex3.Replace(name, string.Empty);

        return name;
    }

    public static string GetRealTypeName(this Type source)
    {
        if (!source.IsGenericType)
        {
            return source.Name;
        }

        var builder = new StringBuilder();

        var stack = new Stack<Type>();

        stack.Push(source);

        while (stack.Count > 0)
        {
            var type = stack.Pop();
            var name = type.Name;

            var lessThanIndex = -1;
            var lessThanCache = -1;

            foreach (var chunk in builder.GetChunks())
            {
                var span = chunk.Span;

                var temp = span.LastIndexOf('<');

                if (temp != -1)
                {
                    lessThanIndex = lessThanCache + temp + 1;
                }

                lessThanCache += span.Length;
            }

            var builderLength = builder.Length;

            var backQuoteIndex = name.IndexOf('`');

            if (backQuoteIndex != -1)
            {
                builder.Append(name[..backQuoteIndex]);
                builder.Append('<');
            }
            else
            {
                builder.Append(name);

                if (builderLength > lessThanIndex + 1)
                {
                    builder.Append('>');
                }

                builder.Append(',');
            }

            foreach (var item in type.GenericTypeArguments.Reverse())
            {
                stack.Push(item);
            }
        }

        builder.Replace(',', '>', builder.Length - 1, 1);

        var realTypeName = builder.ToString();

        return realTypeName;
    }
}