namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTestVectorTable
{
    [TestMethod]
    [Obsolete("nice table")]
    public void TestGetTable()
    {
        const int vectors = 4;
        const int taps    = 23;

        var delayLineLength = taps * 2 + vectors * 2;
        var delayLine       = new float[delayLineLength];
        var delayLineIndex  = 0;

        var input = Enumerable.Range(1, 64).Select(Convert.ToSingle).ToArray();

        var header = $"               {string.Join(", ", Enumerable.Range(0, delayLineLength).Select(s => $"{s,2}"))}";
        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));

        var chunks = input.Length / vectors;

        for (var chunk = 0; chunk < chunks; chunk++)
        {
            var span = input.AsSpan(chunk * vectors, vectors);

            var i0 = delayLineIndex + 0;
            var i1 = delayLineIndex + 1;
            var i2 = delayLineIndex + 2;
            var i3 = delayLineIndex + 3;
            var i4 = (delayLineIndex + taps + vectors + 0) % delayLineLength;
            var i5 = (delayLineIndex + taps + vectors + 1) % delayLineLength;
            var i6 = (delayLineIndex + taps + vectors + 2) % delayLineLength;
            var i7 = (delayLineIndex + taps + vectors + 3) % delayLineLength;

            delayLine[i0] = delayLine[i4] = span[3];
            delayLine[i1] = delayLine[i5] = span[2];
            delayLine[i2] = delayLine[i6] = span[1];
            delayLine[i3] = delayLine[i7] = span[0];

            PrintDelayLine($"chunk: {chunk,2}, span: {string.Join(", ", span.ToArray().Select(s => $"{s,2}"))}, pos: {i0,2}, {i1,2}, {i2,2}, {i3,2}, {i4,2}, {i5,2}, {i6,2}, {i7,2}");

            delayLineIndex -= vectors;
            if (delayLineIndex < 0)
            {
                delayLineIndex += taps + vectors;
            }
        }

        void PrintDelayLine(string? message = null)
        {
            Console.WriteLine($"pos: {delayLineIndex,2}, data: {string.Join(", ", delayLine.Select(s => $"{s,2}"))}, msg: {message}");
        }
    }

    [TestMethod]
    public void TestGetTableOuterInput()
    {
        FilterTable.GetTableOuterInput(23, 4);
    }

    [TestMethod]
    public void TestGetTableOuterDelay()
    {
        FilterTable.GetTableOuterDelay(23, 4);
    }

    [TestMethod]
    public void TestGetTableInnerInput()
    {
        FilterTable.GetTableInnerInput(23, 4);
    }
    
}