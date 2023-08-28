using System.Runtime.InteropServices;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTest4
{
    [TestMethod]
    public unsafe void TestMethod1()
    {
        var f = Formats.Audio.Extensions.FilterState.CreateHalfBand();

        Console.WriteLine(Marshal.OffsetOf<NativeFilter>(nameof(NativeFilter.Coefficients)));
        Console.WriteLine(Marshal.OffsetOf<NativeFilter>(nameof(NativeFilter.CoefficientsLength)));
        Console.WriteLine(Marshal.OffsetOf<NativeFilter>(nameof(NativeFilter.DelayLine)));
        Console.WriteLine(Marshal.OffsetOf<NativeFilter>(nameof(NativeFilter.DelayLineLength)));
        Console.WriteLine(Marshal.OffsetOf<NativeFilter>(nameof(NativeFilter.Taps)));
        Console.WriteLine(Marshal.OffsetOf<NativeFilter>(nameof(NativeFilter.TapsLength)));
        Console.WriteLine(Marshal.OffsetOf<NativeFilter>(nameof(NativeFilter.Position)));
        Console.WriteLine();
        Console.WriteLine(sizeof(NativeFilter));

        using (var nf = new NativeFilter(f))
        {
            Console.WriteLine((IntPtr)nf.Coefficients);
            Console.WriteLine((IntPtr)nf.DelayLine);
            Console.WriteLine((IntPtr)nf.Taps);
        }

        int* fp = (int*)1234;
    }
}