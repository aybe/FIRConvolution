namespace FIRConvolution.Tests.Unsorted;

public interface IValueTrack<T> where T : unmanaged
{
    ref T this[in int index] { get; }
}

public class ValueTrack<T> : IValueTrack<T> where T : unmanaged
{
    private readonly T[] Array;

    public readonly int Count;

    private int Index;

    public ValueTrack(in int count)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        Array = new T[count];
        Count = count;
        Index = 0;
    }

    public ref T this[in int index]
    {
        get
        {
            var count = Index + index;

            if (count >= Count)
            {
                count -= Count;
            }

            return ref Array[count];
        }
    }

    public void Push(in T item)
    {
        Array[Index] = item;

        Index++;

        if (Index == Count)
        {
            Index = 0;
        }
    }

    public void Print()
    {
        for (var i = 0; i < Count; i++)
        {
            Console.WriteLine($"Index: {i}, Value: {this[i]}");
        }
    }
}