using System.Numerics;

namespace Heir.Tests;

public static class AssertExtensions
{
    public static void FuzzyEqual<T>(T expected, T actual, double tolerance = 0.0000000001) where T : INumber<T>
    {
        if (Math.Abs(Convert.ToDouble(expected - actual)) > tolerance)
            throw new Xunit.Sdk.XunitException($"Expected {actual} to be approximately equal to {expected} within a tolerance of {tolerance}.");
    }
}