using System.Diagnostics.CodeAnalysis;

namespace FIRConvolution.Extensions;

[SuppressMessage("ReSharper", "UnusedParameter.Global")]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
public static class AssertExtensions
{
    public static void IsGreaterThan(
        this Assert assert, int expected, int actual, string? message = null)
    {
        if (actual > expected)
        {
            return;
        }

        Assert.Fail(message ?? $"{actual} is expected to be greater than {expected}.");
    }

    public static void IsGreaterThanOrEqual(
        this Assert assert, int expected, int actual, string? message = null)
    {
        if (actual >= expected)
        {
            return;
        }

        Assert.Fail(message ?? $"{actual} is expected to be greater than or equal to {expected}.");
    }

    public static void IsLessThan(
        this Assert assert, int expected, int actual, string? message = null)
    {
        if (actual < expected)
        {
            return;
        }

        Assert.Fail(message ?? $"{actual} is expected to be less than {expected}.");
    }

    public static void IsLessThanOrEqual(
        this Assert assert, int expected, int actual, string? message = null)
    {
        if (actual <= expected)
        {
            return;
        }

        Assert.Fail(message ?? $"{actual} is expected to be less than or equal to {expected}.");
    }
}