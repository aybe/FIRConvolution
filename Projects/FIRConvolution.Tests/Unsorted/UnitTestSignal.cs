using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using FIRConvolution.Tests.Formats.Audio.Extensions;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public partial class UnitTestSignal
{
    [TestMethod]
    public void Test1()
    {
        Process(GetActions1(), @"C:\Temp\Filter1.csv");
    }

    [TestMethod]
    public void Test2()
    {
        Process(GetActions2(), @"C:\Temp\Filter2.csv");
    }

    private static void Process(Func<HalfBandFirFilter, Action<float[], float[], int>>[] actions, string path)
    {
        const int length = 128;

        var source = GenerateUnitImpulse(length, 0);
        
        source[0] = 1;


        var target = new float[source.Length];

        var coefficients = FilterState.CreateHalfBand(44100, 8820).Coefficients;

        //Console.WriteLine(coefficients.Length);
        //Console.WriteLine();
        //UnitTestRounding.PrintOffsets(coefficients.Length, 4);
        Console.WriteLine();
        var list = new List<float[]>();

        foreach (var action in actions)
        {
            Array.Clear(target);

            var filter = new HalfBandFirFilter(coefficients);

            var process = action(filter);

            process(source, target, length);

            list.Add(target.ToArray());
        }

        Console.WriteLine();
        var builder = new StringBuilder();
        for (var i = 0; i < length; i++)
        {
            foreach (var floats in list)
            {
                var f = floats[i];
                builder.Append($"{f,14}, ");
            }

            builder.AppendLine();
        }

        var s1 = builder.ToString();

        Console.WriteLine(s1);

        File.WriteAllText(path, s1);
    }

    private static Func<HalfBandFirFilter, Action<float[], float[], int>>[] GetActions1()
    {
        return new[]
        {
            GetFunction(s => s.VectorizedNormal),
            GetFunction(s => s.VectorizedInner),
            GetFunction(s => s.VectorizedOuter),
            GetFunction(s => s.VectorizedOuterInner)
        };
    }

    private static Func<HalfBandFirFilter, Action<float[], float[], int>>[] GetActions2()
    {
        return new[]
        {
            GetFunction(s => s.VectorizedNormal),
            GetFunction(s => s.VectorizedInnerSimple),
            GetFunction(s => s.VectorizedInnerVector4),
            GetFunction(s => s.VectorizedOuterInner2),
        };
    }

    private static Func<HalfBandFirFilter, Action<float[], float[], int>> GetFunction(
        Expression<Func<HalfBandFirFilter, Action<float[], float[], int>>> expression)
    {
        var compile = expression.Compile();

        return compile;
    }

    public static float[] GenerateUnitImpulse(int length, int impulsePosition)
    {
        var impulse = new float[length];
        for (var i = 0; i < length; i++)
        {
            if (i == impulsePosition)
            {
                impulse[i] = 1.0f;
            }
            else
            {
                impulse[i] = 0.0f;
            }
        }

        return impulse;
    }

    [TestMethod]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "TooWideLocalVariableScope")]
    public void Test3()
    {
        // https://gist.github.com/vermorel/7ad35212df44f3a79bca8ab5fe8e7622
        // TODO kernel should be odd
        var @in           = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var input_length  = @in.Length;
        var kernel        = new[] { 0, 0, 0, 2f, 0, 0, 0 };
        var kernel_length = kernel.Length;
        var @out          = new float[@in.Length + (kernel_length - 1)]; // BUG?

        Span<float> in_padded = stackalloc float[input_length + 8];

        in_padded.Clear();
        Console.WriteLine("input");
        Console.WriteLine(string.Join(", ", @in));
        Console.WriteLine();

        @in.CopyTo(in_padded.Slice(4));
        Console.WriteLine("input padded");
        Console.WriteLine(string.Join(", ", in_padded.ToArray()));

        Console.WriteLine();

        Span<Vector4> kernel_many = stackalloc Vector4[kernel_length];
        for (var index = 0; index < kernel_length; index++)
        {
            kernel_many[index] = new Vector4(kernel[index]);
        }

        Console.WriteLine("kernel_many");
        Console.WriteLine(string.Join(", ", kernel_many.ToArray()));
        Console.WriteLine();

        Console.WriteLine("float as vec4");
        var cast = MemoryMarshal.Cast<float, Vector4>(in_padded);
        Console.WriteLine(string.Join(", ", cast.ToArray()));

        Console.WriteLine();
        Console.WriteLine("loop");
        Vector4 acc, block, prod;
        var     i = 0;
        for (; i < input_length + kernel_length - 4; i += 4)
        {
            acc = Vector4.Zero;

            var startk = i > input_length - 1 ? i - (input_length - 1) : 0;
            var endk   = i + 3 < kernel_length ? i + 3 : kernel_length - 1;

            for (var k = startk; k <= endk; k++)
            {
                var index = 4 + i - k;
                block =  new Vector4(in_padded.Slice(index));
                prod  =  block * kernel_many[k];
                acc   += prod;
                Console.WriteLine(
                    $"\t{nameof(i)}: {i}, {nameof(k)}: {k}, {nameof(index)}: {index}, " +
                    $"{nameof(block)}: {block}, {nameof(prod)}: {prod}, {nameof(acc)}: {acc}");

                //block = in_padded[index];
            }

            for (var j = 0; j < 4; j++) //@out[i] = acc; // TODO
            {
                @out[i + j] = acc[j];
            }
        }

        for (; i < input_length + kernel_length - 1; i++)
        {
            @out[i] = 0.12345678f;
            var startk = i >= input_length ? i - input_length + 1 : 0;
            var endk   = i < kernel_length ? i : kernel_length - 1;
            for (var k = startk; k <= endk; k++)
            {
                @out[i] += @in[i - k] * kernel[k];
            }
        }

        Console.WriteLine("output");
        Console.WriteLine(string.Join(Environment.NewLine, @out.Select((s, t) => $"{t,3}: {s:F8}")));
        Console.WriteLine();
    }
}