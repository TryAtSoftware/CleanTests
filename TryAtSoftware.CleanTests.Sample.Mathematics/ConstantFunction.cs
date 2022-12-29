namespace TryAtSoftware.CleanTests.Sample.Mathematics;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(MathConstants.Category, "Constant function")]
public class ConstantFunction : IMathFunction
{
    public double SolveFor(double x) => 1;
}