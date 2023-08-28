namespace FIRConvolution.Tests.Unsorted;

[Serializable]
public sealed class NativeBufferI32 : NativeBuffer<int>
{
    public unsafe NativeBufferI32(int count, int* array) : base(count, array)
    {
    }
}