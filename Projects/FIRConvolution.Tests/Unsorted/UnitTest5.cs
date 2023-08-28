using FIRConvolution.Tests.Extensions;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTest5
{
    [TestMethod]
    public void TestMethod1()
    {
        var dictionary = new Dictionary<Dictionary<int, float>, Dictionary<long, double>>();

        var realTypeName = dictionary.GetType().GetRealTypeName();

        Console.WriteLine(realTypeName);

        Assert.AreEqual("Dictionary<Dictionary<Int32,Single>,Dictionary<Int64,Double>>", realTypeName);
    }

    [TestMethod]
    public void TestMethod2()
    {
        var dictionary = new Dictionary<Dictionary<int, float>, Dictionary<long, double>>();

        Console.WriteLine(TypeExtensions.GetRealTypeName(dictionary.GetType(), true));
        Console.WriteLine();
        Console.WriteLine(TypeExtensions.GetRealTypeName(dictionary.GetType(), false));

        Console.WriteLine(typeof(int).GetRealTypeName(false));
        Console.WriteLine(typeof(int).GetRealTypeName(true));
    }
}