using System.Runtime.InteropServices;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTestAlignment
{
    [TestMethod]
    public void TestAlignment()
    {
        var requested   = 100;
        var pointerSize = IntPtr.Size;

        Console.WriteLine($"{nameof(pointerSize)}: {pointerSize}");
        Console.WriteLine($"{nameof(requested)}: {requested}");
        Console.WriteLine();

        for (var address = 16; address <= 32; address++)
        {
            for (var alignment = 1; alignment <= 8; alignment++)
            {
                var alignment2 = 1 << alignment - 1;
                if (alignment2 > 8)
                {
                    continue;
                }

                var size = requested + pointerSize + (alignment2 - 1);

                var alignedAddress = address + (alignment2 - 1) + pointerSize & ~(alignment2 - 1);

                var originalPointer = alignedAddress - pointerSize;
                Assert.IsTrue(originalPointer >= address);
                Assert.IsTrue(originalPointer == alignedAddress - pointerSize);
                Console.WriteLine(
                    $"{nameof(address)}: {address,3}, " +
                    $"{nameof(alignment2)}: {alignment2,3}, " +
                    $"{nameof(alignedAddress)}: {alignedAddress,3}, " +
                    $"{nameof(originalPointer)}: {originalPointer,3}, " +
                    $"{nameof(size)}: {size,3}, " +
                    $""
                );
            }

            Console.WriteLine();
        }
    }

    private IntPtr alloc(int size, int alignment)
    {
        // Allocate the memory plus a bit extra for alignment and storing the adjustment value
        var originalPtr = Marshal.AllocHGlobal(size + alignment + IntPtr.Size);

        var originalAddress = originalPtr.ToInt64();

        // Calculate the adjustment needed to keep the memory aligned
        var adjust = alignment - (originalAddress & ~(alignment - 1));

        // Calculate the aligned memory location
        var alignedPtr = new IntPtr(originalAddress + adjust);

        // Store the adjustment value at the memory location immediately before the aligned memory
        Marshal.WriteIntPtr(new IntPtr(originalAddress + adjust - IntPtr.Size), new IntPtr(adjust));

        return alignedPtr;
    }

    private void Free(IntPtr alignedPtr)
    {
        var alignedAddress = alignedPtr.ToInt64();

        // Read the adjustment value that was stored immediately before the aligned memory
        var adjust = Marshal.ReadIntPtr(new IntPtr(alignedAddress - IntPtr.Size)).ToInt64();

        // Calculate the original pointer
        var originalPtr = new IntPtr(alignedAddress - adjust);

        // Free the memory
        Marshal.FreeHGlobal((IntPtr)(alignedAddress - adjust));
    }
}