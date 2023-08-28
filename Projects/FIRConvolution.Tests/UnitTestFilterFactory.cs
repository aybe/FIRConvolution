namespace FIRConvolution.Tests;

public delegate T UnitTestFilterFactory<out T>(float[] h) where T : Filter;