namespace FIRConvolution.Tests;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class UnitTestFilterAttribute : Attribute
{
    public UnitTestFilterAttribute(double bandwidth)
    {
        Bandwidth = bandwidth;
    }

    public double Bandwidth { get; }

    public override string ToString()
    {
        return $"{nameof(Bandwidth)}: {Bandwidth}";
    }
}