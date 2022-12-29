namespace TryAtSoftware.CleanTests.Sample.Mathematics;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(MathConstants.Category, "Square of a number")]
public class SquareFunction : IMathFunction
{
    public double SolveFor(double x) => x * x;
}