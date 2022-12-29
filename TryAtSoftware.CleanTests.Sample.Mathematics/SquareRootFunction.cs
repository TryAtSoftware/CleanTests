namespace TryAtSoftware.CleanTests.Sample.Mathematics;

using System;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(MathConstants.Category, "Square root of a number")]
public class SquareRootFunction : IMathFunction
{
    public double SolveFor(double x) => Math.Sqrt(x);
}