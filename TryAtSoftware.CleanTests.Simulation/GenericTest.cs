namespace TryAtSoftware.CleanTests.Simulation;

using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Simulation.Attributes;
using Xunit.Abstractions;

public class GenericTest<[Numeric] T> : CleanTest
    where T : notnull
{
    public GenericTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [CleanFact]
    public void StandardFact() => Assert.Equal(4, 2 + 2);
}