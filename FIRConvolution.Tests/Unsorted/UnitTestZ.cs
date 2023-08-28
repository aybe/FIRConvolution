using System.Numerics;

namespace FIRConvolution.Tests.Unsorted;

[TestClass]
public class UnitTestZ
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void TestHCenterModulo()
    {
        for (var i = 3; i <= 461; i += 2)
        {
            var c = i / 2;
            var b = c % 2 == 1;
            TestContext.WriteLine($"taps: {i,3}, center: {c,3}, odd: {b}");
            if (b)
            {
            }
        }
    }

    [TestMethod]
    public void Test()
    {
        var a = 1.0f;
        var b = 2.0f;
        var c = 3.0f;
        var d = 4.0f;

        var x = new Vector4(a * a, b * b, c * c, d * d);

        var y = new Vector4(a, b, c, d);

        var z = y * y;

        var w = Vector4.Multiply(y, y);
    }
}