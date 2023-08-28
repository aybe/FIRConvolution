namespace FIRConvolution.Tests.Unsorted;

[Serializable]
public abstract unsafe class NativeBuffer<T> where T : unmanaged
{
    public T*  Array;
    public int Count;

    protected NativeBuffer(int count, T* array)
    {
        Count = count;
        Array = array;
    }


    public override string ToString()
    {
        return $"{nameof(Count)}: {Count}";
    }
}