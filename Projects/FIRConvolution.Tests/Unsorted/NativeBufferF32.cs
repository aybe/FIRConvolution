namespace FIRConvolution.Tests.Unsorted;

[Serializable]
public sealed class NativeBufferF32 : NativeBuffer<float>
{
    public unsafe NativeBufferF32(int count, float* array) : base(count, array)
    {
    }
}