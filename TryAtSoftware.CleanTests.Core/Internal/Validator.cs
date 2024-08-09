namespace TryAtSoftware.CleanTests.Core.Internal;

using System;

internal static class Validator
{
    public static void ValidateMaxDegreeOfParallelism(int value)
    {
        if (value <= 0) throw new ArgumentException("The max degree of parallelism should be always positive.");
    }
}